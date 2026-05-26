using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Admin;
using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Contracts.Security;
using TourGuideMarketplace.Domain.Trust;

namespace TourGuideMarketplace.Application.Services;

public sealed class AdminTrustService : IAdminTrustService
{
    private readonly IGuideProfileRepository _guideProfileRepository;
    private readonly ITrustRepository _trustRepository;
    private readonly IUserAccountService _userAccountService;

    public AdminTrustService(
        IGuideProfileRepository guideProfileRepository,
        ITrustRepository trustRepository,
        IUserAccountService userAccountService)
    {
        _guideProfileRepository = guideProfileRepository;
        _trustRepository = trustRepository;
        _userAccountService = userAccountService;
    }

    public async Task<Result<PagedResult<AdminVerificationSummaryResponse>>> SearchAsync(
        AdminVerificationSearchRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureVerificationsForExistingUsersAsync(cancellationToken);

        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        UserVerificationStatus? status = null;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<UserVerificationStatus>(request.Status.Trim(), ignoreCase: true, out var parsedStatus))
            {
                return Result<PagedResult<AdminVerificationSummaryResponse>>.Failure("Unknown verification status.");
            }

            status = parsedStatus;
        }

        var totalCount = await _trustRepository.CountUserVerificationsAsync(status, cancellationToken);
        var verifications = await _trustRepository.ListUserVerificationsAsync(status, pageNumber, pageSize, cancellationToken);
        var userIds = verifications.Select(verification => verification.UserId).ToArray();
        var users = (await _userAccountService.ListByIdsAsync(userIds, cancellationToken))
            .ToDictionary(user => user.Id);
        var attempts = await _trustRepository.ListIdentityAttemptsAsync(userIds, cancellationToken);
        var lastAttemptByUser = attempts
            .GroupBy(attempt => attempt.UserId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(attempt => attempt.RequestedAt).First());

        var items = new List<AdminVerificationSummaryResponse>();
        foreach (var verification in verifications)
        {
            if (!users.TryGetValue(verification.UserId, out var user))
            {
                continue;
            }

            var roles = await _userAccountService.GetRolesAsync(user.Id, cancellationToken);
            lastAttemptByUser.TryGetValue(user.Id, out var lastAttempt);
            items.Add(new AdminVerificationSummaryResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                roles.ToArray(),
                verification.Status.ToString(),
                user.EmailConfirmed || verification.EmailVerifiedAt.HasValue,
                user.PhoneNumberConfirmed || verification.PhoneVerifiedAt.HasValue,
                verification.IdentityVerifiedAt.HasValue,
                verification.ProfileValidatedAt.HasValue,
                verification.IdentityProvider,
                lastAttempt?.Status.ToString(),
                verification.CreatedAt,
                verification.UpdatedAt));
        }

        return Result<PagedResult<AdminVerificationSummaryResponse>>.Success(new PagedResult<AdminVerificationSummaryResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount));
    }

    public async Task<Result<AdminVerificationDetailResponse>> GetDetailAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<AdminVerificationDetailResponse>.Failure("User was not found.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        SyncContactFlags(user, verification);
        UpdateDerivedStatus(user, verification);
        await _trustRepository.SaveChangesAsync(cancellationToken);

        return Result<AdminVerificationDetailResponse>.Success(await MapDetailAsync(user, verification, cancellationToken));
    }

    public async Task<Result<AdminVerificationDetailResponse>> ApproveProfileAsync(
        Guid adminUserId,
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await GetGuideUserAndVerificationAsync(userId, cancellationToken);
        if (!result.Succeeded)
        {
            return Result<AdminVerificationDetailResponse>.Failure(result.Errors.ToArray());
        }

        var context = result.Value!;
        var user = context.User;
        var verification = context.Verification;
        if (!verification.IdentityVerifiedAt.HasValue)
        {
            return Result<AdminVerificationDetailResponse>.Failure("Identity must be verified before validating the profile.");
        }

        var profile = await _guideProfileRepository.GetByUserIdAsync(user.Id, asTracking: true, cancellationToken);
        if (profile is null)
        {
            return Result<AdminVerificationDetailResponse>.Failure("Guide profile was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        verification.Status = UserVerificationStatus.ProfileValidated;
        verification.ProfileValidatedAt = now;
        verification.InReviewReason = null;
        profile.IsVerified = true;

        await ResolveOpenCasesAsync(
            user.Id,
            AdminReviewCaseType.ProfileValidation,
            adminUserId,
            request.Notes,
            "Profile validated.",
            cancellationToken);

        await _trustRepository.SaveChangesAsync(cancellationToken);
        return Result<AdminVerificationDetailResponse>.Success(await MapDetailAsync(user, verification, cancellationToken));
    }

    public async Task<Result<AdminVerificationDetailResponse>> RejectProfileAsync(
        Guid adminUserId,
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await GetGuideUserAndVerificationAsync(userId, cancellationToken);
        if (!result.Succeeded)
        {
            return Result<AdminVerificationDetailResponse>.Failure(result.Errors.ToArray());
        }

        var context = result.Value!;
        var user = context.User;
        var verification = context.Verification;
        var reason = NormalizeReason(request.Reason, "Guide profile requires changes before it can be validated.");
        var profile = await _guideProfileRepository.GetByUserIdAsync(user.Id, asTracking: true, cancellationToken);

        if (profile is not null)
        {
            profile.IsVerified = false;
        }

        verification.Status = UserVerificationStatus.InReview;
        verification.InReviewReason = reason;
        verification.ProfileValidatedAt = null;

        await UpsertOpenCaseAsync(
            user.Id,
            AdminReviewCaseType.ProfileValidation,
            reason,
            request.Notes,
            cancellationToken);

        await _trustRepository.SaveChangesAsync(cancellationToken);
        return Result<AdminVerificationDetailResponse>.Success(await MapDetailAsync(user, verification, cancellationToken));
    }

    public async Task<Result<AdminVerificationDetailResponse>> SuspendAsync(
        Guid adminUserId,
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<AdminVerificationDetailResponse>.Failure("User was not found.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        var reason = NormalizeReason(request.Reason, "Account suspended by an administrator.");
        var now = DateTimeOffset.UtcNow;

        verification.Status = UserVerificationStatus.Suspended;
        verification.SuspendedAt = now;
        verification.SuspendedReason = reason;
        verification.InReviewReason = null;

        var activeResult = await _userAccountService.SetActiveAsync(user.Id, isActive: false, cancellationToken);
        if (!activeResult.Succeeded)
        {
            return Result<AdminVerificationDetailResponse>.Failure(activeResult.Errors.ToArray());
        }

        var profile = await _guideProfileRepository.GetByUserIdAsync(user.Id, asTracking: true, cancellationToken);
        if (profile is not null)
        {
            profile.IsVerified = false;
        }

        await UpsertOpenCaseAsync(
            user.Id,
            AdminReviewCaseType.Suspension,
            reason,
            request.Notes,
            cancellationToken);

        await _trustRepository.SaveChangesAsync(cancellationToken);
        user = user with { IsActive = false };
        return Result<AdminVerificationDetailResponse>.Success(await MapDetailAsync(user, verification, cancellationToken));
    }

    public async Task<Result<AdminVerificationDetailResponse>> ReactivateAsync(
        Guid adminUserId,
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<AdminVerificationDetailResponse>.Failure("User was not found.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        var activeResult = await _userAccountService.SetActiveAsync(user.Id, isActive: true, cancellationToken);
        if (!activeResult.Succeeded)
        {
            return Result<AdminVerificationDetailResponse>.Failure(activeResult.Errors.ToArray());
        }

        verification.SuspendedAt = null;
        verification.SuspendedReason = null;
        verification.ProfileValidatedAt = null;
        verification.InReviewReason = null;

        var profile = await _guideProfileRepository.GetByUserIdAsync(user.Id, asTracking: true, cancellationToken);
        if (profile is not null)
        {
            profile.IsVerified = false;
        }

        user = user with { IsActive = true };
        SyncContactFlags(user, verification);
        SetStatusAfterReactivation(user, verification);

        await ResolveOpenCasesAsync(
            user.Id,
            AdminReviewCaseType.Suspension,
            adminUserId,
            request.Notes,
            "Account reactivated.",
            cancellationToken);

        await _trustRepository.SaveChangesAsync(cancellationToken);
        return Result<AdminVerificationDetailResponse>.Success(await MapDetailAsync(user, verification, cancellationToken));
    }

    private async Task<Result<GuideVerificationContext>> GetGuideUserAndVerificationAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<GuideVerificationContext>.Failure("User was not found.");
        }

        if (!await _userAccountService.IsInRoleAsync(user.Id, AppRoles.Guide, cancellationToken))
        {
            return Result<GuideVerificationContext>.Failure("Only guide profiles can be validated.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        return Result<GuideVerificationContext>.Success(new GuideVerificationContext(user, verification));
    }

    private async Task<AdminVerificationDetailResponse> MapDetailAsync(
        UserAccount user,
        UserVerification verification,
        CancellationToken cancellationToken)
    {
        var roles = await _userAccountService.GetRolesAsync(user.Id, cancellationToken);
        var attempts = (await _trustRepository.ListIdentityAttemptsForUserAsync(user.Id, cancellationToken))
            .OrderByDescending(attempt => attempt.RequestedAt)
            .Select(attempt => new AdminVerificationAttemptResponse(
                attempt.Id,
                attempt.Provider,
                attempt.ExternalVerificationId,
                attempt.Status.ToString(),
                attempt.Country,
                attempt.DocumentType,
                attempt.DocumentNumberLast4,
                attempt.FailureReason,
                attempt.RequestedAt,
                attempt.CompletedAt))
            .ToArray();

        var cases = (await _trustRepository.ListReviewCasesForUserAsync(user.Id, cancellationToken))
            .OrderByDescending(reviewCase => reviewCase.Status == AdminReviewCaseStatus.Open)
            .ThenByDescending(reviewCase => reviewCase.CreatedAt)
            .Select(reviewCase => new AdminReviewCaseResponse(
                reviewCase.Id,
                reviewCase.Type.ToString(),
                reviewCase.Status.ToString(),
                reviewCase.Reason,
                reviewCase.Notes,
                reviewCase.Decision,
                reviewCase.ResolvedByUserId,
                reviewCase.ResolvedAt,
                reviewCase.CreatedAt))
            .ToArray();

        return new AdminVerificationDetailResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            roles.ToArray(),
            TrustStatusMapper.Map(user, verification, roles.ToArray()),
            attempts,
            cases);
    }

    private async Task EnsureVerificationsForExistingUsersAsync(CancellationToken cancellationToken)
    {
        var existingUserIds = await _trustRepository.ListUserIdsWithVerificationAsync(cancellationToken);
        var existingSet = existingUserIds.ToHashSet();
        var users = await _userAccountService.ListAllAsync(cancellationToken);

        foreach (var user in users.Where(user => !existingSet.Contains(user.Id)))
        {
            var verification = new UserVerification { UserId = user.Id };
            SyncContactFlags(user, verification);
            UpdateDerivedStatus(user, verification);
            _trustRepository.AddUserVerification(verification);
        }

        await _trustRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<UserVerification> EnsureVerificationAsync(
        UserAccount user,
        CancellationToken cancellationToken)
    {
        var verification = await _trustRepository.GetUserVerificationAsync(user.Id, asTracking: true, cancellationToken);

        if (verification is not null)
        {
            return verification;
        }

        verification = new UserVerification { UserId = user.Id };
        SyncContactFlags(user, verification);
        UpdateDerivedStatus(user, verification);
        _trustRepository.AddUserVerification(verification);
        return verification;
    }

    private async Task UpsertOpenCaseAsync(
        Guid userId,
        AdminReviewCaseType type,
        string reason,
        string? notes,
        CancellationToken cancellationToken)
    {
        var existingCase = await _trustRepository.GetOpenReviewCaseAsync(userId, type, cancellationToken);

        if (existingCase is not null)
        {
            existingCase.Reason = reason;
            existingCase.Notes = notes;
            return;
        }

        _trustRepository.AddAdminReviewCase(new AdminReviewCase
        {
            UserId = userId,
            Type = type,
            Reason = reason,
            Notes = notes
        });
    }

    private async Task ResolveOpenCasesAsync(
        Guid userId,
        AdminReviewCaseType type,
        Guid adminUserId,
        string? notes,
        string decision,
        CancellationToken cancellationToken)
    {
        var openCases = await _trustRepository.ListOpenReviewCasesAsync(userId, type, cancellationToken);

        foreach (var reviewCase in openCases)
        {
            reviewCase.Status = AdminReviewCaseStatus.Resolved;
            reviewCase.Decision = decision;
            reviewCase.Notes = notes ?? reviewCase.Notes;
            reviewCase.ResolvedAt = DateTimeOffset.UtcNow;
            reviewCase.ResolvedByUserId = adminUserId;
        }
    }

    private static void SyncContactFlags(UserAccount user, UserVerification verification)
    {
        var now = DateTimeOffset.UtcNow;
        if (user.EmailConfirmed)
        {
            verification.EmailVerifiedAt ??= now;
        }

        if (user.PhoneNumberConfirmed)
        {
            verification.PhoneVerifiedAt ??= now;
        }
    }

    private static void UpdateDerivedStatus(UserAccount user, UserVerification verification)
    {
        if (verification.Status == UserVerificationStatus.Suspended || verification.SuspendedAt.HasValue)
        {
            verification.Status = UserVerificationStatus.Suspended;
            return;
        }

        if (verification.Status == UserVerificationStatus.InReview)
        {
            return;
        }

        if (verification.ProfileValidatedAt.HasValue)
        {
            verification.Status = UserVerificationStatus.ProfileValidated;
            return;
        }

        if (verification.IdentityVerifiedAt.HasValue)
        {
            verification.Status = UserVerificationStatus.IdentityVerified;
            return;
        }

        verification.Status = (user.EmailConfirmed || verification.EmailVerifiedAt.HasValue)
            && (user.PhoneNumberConfirmed || verification.PhoneVerifiedAt.HasValue)
                ? UserVerificationStatus.ContactVerified
                : UserVerificationStatus.Unverified;
    }

    private static void SetStatusAfterReactivation(UserAccount user, UserVerification verification)
    {
        if (verification.IdentityVerifiedAt.HasValue)
        {
            verification.Status = UserVerificationStatus.IdentityVerified;
            return;
        }

        verification.Status = (user.EmailConfirmed || verification.EmailVerifiedAt.HasValue)
            && (user.PhoneNumberConfirmed || verification.PhoneVerifiedAt.HasValue)
                ? UserVerificationStatus.ContactVerified
                : UserVerificationStatus.Unverified;
    }

    private static string NormalizeReason(string? reason, string fallback)
    {
        return string.IsNullOrWhiteSpace(reason) ? fallback : reason.Trim();
    }

    private sealed record GuideVerificationContext(UserAccount User, UserVerification Verification);
}
