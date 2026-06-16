using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Application.Trust;
using TourGuideMarketplace.Contracts.Security;
using TourGuideMarketplace.Contracts.Trust;
using TourGuideMarketplace.Domain.Guides;
using TourGuideMarketplace.Domain.Trust;

namespace TourGuideMarketplace.Application.Services;

public sealed class TrustService : ITrustService
{
    private const int ContactCodeMinutes = 10;
    private const int MaxContactCodeAttempts = 5;

    private readonly IGuideProfileRepository _guideProfileRepository;
    private readonly IIdentityVerificationProvider _identityVerificationProvider;
    private readonly ITrustRepository _trustRepository;
    private readonly IUserAccountService _userAccountService;

    public TrustService(
        IGuideProfileRepository guideProfileRepository,
        IIdentityVerificationProvider identityVerificationProvider,
        ITrustRepository trustRepository,
        IUserAccountService userAccountService)
    {
        _guideProfileRepository = guideProfileRepository;
        _identityVerificationProvider = identityVerificationProvider;
        _trustRepository = trustRepository;
        _userAccountService = userAccountService;
    }

    public async Task<Result<TrustStatusResponse>> GetMyStatusAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<TrustStatusResponse>.Failure("User was not found.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        SyncContactFlags(user, verification);
        UpdateDerivedStatus(user, verification);
        await _trustRepository.SaveChangesAsync(cancellationToken);

        var roles = await _userAccountService.GetRolesAsync(user.Id, cancellationToken);
        return Result<TrustStatusResponse>.Success(TrustStatusMapper.Map(user, verification, roles.ToArray()));
    }

    public Task<Result<ContactVerificationResponse>> RequestEmailVerificationAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return RequestContactVerificationAsync(userId, ContactVerificationChannel.Email, cancellationToken);
    }

    public Task<Result<ContactVerificationResponse>> RequestPhoneVerificationAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return RequestContactVerificationAsync(userId, ContactVerificationChannel.Phone, cancellationToken);
    }

    public Task<Result<TrustStatusResponse>> ConfirmEmailVerificationAsync(
        Guid userId,
        ConfirmContactVerificationRequest request,
        CancellationToken cancellationToken)
    {
        return ConfirmContactVerificationAsync(userId, ContactVerificationChannel.Email, request, cancellationToken);
    }

    public Task<Result<TrustStatusResponse>> ConfirmPhoneVerificationAsync(
        Guid userId,
        ConfirmContactVerificationRequest request,
        CancellationToken cancellationToken)
    {
        return ConfirmContactVerificationAsync(userId, ContactVerificationChannel.Phone, request, cancellationToken);
    }

    public async Task<Result<IdentityVerificationResponse>> StartIdentityVerificationAsync(
        Guid userId,
        StartIdentityVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<IdentityVerificationResponse>.Failure("User was not found.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        SyncContactFlags(user, verification);

        if (!IsContactVerified(user, verification))
        {
            return Result<IdentityVerificationResponse>.Failure("Email and phone must be verified before starting identity verification.");
        }

        var validationErrors = ValidateIdentityRequest(request);
        if (validationErrors.Count > 0)
        {
            return Result<IdentityVerificationResponse>.Failure(validationErrors.ToArray());
        }

        var providerResult = await _identityVerificationProvider.StartAsync(
            new IdentityVerificationProviderRequest(
                user.Id,
                user.FullName,
                request.Country.Trim(),
                request.DocumentType.Trim(),
                request.DocumentNumber.Trim(),
                request.DateOfBirth,
                request.RequestedMockOutcome),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        _trustRepository.AddIdentityVerificationAttempt(new IdentityVerificationAttempt
        {
            UserId = user.Id,
            Provider = providerResult.Provider,
            ExternalVerificationId = providerResult.ExternalVerificationId,
            Status = MapAttemptStatus(providerResult.Status),
            Country = request.Country.Trim(),
            DocumentType = request.DocumentType.Trim(),
            DocumentNumberLast4 = GetLast4(request.DocumentNumber),
            FailureReason = providerResult.FailureReason,
            RequestPayloadJson = providerResult.RequestPayloadJson,
            ResponsePayloadJson = providerResult.ResponsePayloadJson,
            RequestedAt = now,
            CompletedAt = now
        });

        verification.IdentityProvider = providerResult.Provider;
        verification.ExternalVerificationId = providerResult.ExternalVerificationId;
        verification.IdentityStartedAt ??= now;

        if (providerResult.Status == IdentityVerificationProviderStatus.Approved)
        {
            verification.IdentityVerifiedAt = now;
            verification.InReviewReason = null;
            verification.Status = UserVerificationStatus.IdentityVerified;
        }
        else
        {
            verification.Status = UserVerificationStatus.InReview;
            verification.InReviewReason = providerResult.FailureReason ?? "Identity verification requires manual review.";
            await CreateReviewCaseIfMissingAsync(
                user.Id,
                AdminReviewCaseType.IdentityVerification,
                verification.InReviewReason,
                cancellationToken);
        }

        await _trustRepository.SaveChangesAsync(cancellationToken);

        var roles = await _userAccountService.GetRolesAsync(user.Id, cancellationToken);
        var status = TrustStatusMapper.Map(user, verification, roles.ToArray());

        return Result<IdentityVerificationResponse>.Success(new IdentityVerificationResponse(
            providerResult.Provider,
            providerResult.ExternalVerificationId,
            providerResult.Status.ToString(),
            providerResult.FailureReason,
            status));
    }

    public async Task<Result<TrustStatusResponse>> AcceptRulesAsync(
        Guid userId,
        AcceptTrustRulesRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.AcceptCodeOfConduct || !request.AcceptSafetyRules)
        {
            return Result<TrustStatusResponse>.Failure("Code of conduct and safety rules must be accepted.");
        }

        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<TrustStatusResponse>.Failure("User was not found.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        verification.CodeOfConductAcceptedAt ??= now;
        verification.SafetyRulesAcceptedAt ??= now;
        SyncContactFlags(user, verification);
        UpdateDerivedStatus(user, verification);

        await _trustRepository.SaveChangesAsync(cancellationToken);

        var roles = await _userAccountService.GetRolesAsync(user.Id, cancellationToken);
        return Result<TrustStatusResponse>.Success(TrustStatusMapper.Map(user, verification, roles.ToArray()));
    }

    public async Task<Result<TrustStatusResponse>> SubmitGuideProfileReviewAsync(
        Guid userId,
        SubmitManualGuideReviewRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<TrustStatusResponse>.Failure("User was not found.");
        }

        if (!await _userAccountService.IsInRoleAsync(user.Id, AppRoles.Guide, cancellationToken))
        {
            return Result<TrustStatusResponse>.Failure("Only guide users can submit a guide profile for review.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        SyncContactFlags(user, verification);

        if (!ManualReviewMapper.IsEmailConfirmed(user, verification))
        {
            return Result<TrustStatusResponse>.Failure("Email must be confirmed before submitting a guide profile for review.");
        }

        if (!verification.CodeOfConductAcceptedAt.HasValue || !verification.SafetyRulesAcceptedAt.HasValue)
        {
            return Result<TrustStatusResponse>.Failure("Code of conduct and safety rules must be accepted before profile review.");
        }

        var profile = await _guideProfileRepository.GetByUserIdAsync(userId, asTracking: true, cancellationToken);
        if (profile is null)
        {
            return Result<TrustStatusResponse>.Failure("Guide profile must be completed before profile review.");
        }

        var profileErrors = ValidateGuideProfile(profile);
        if (profileErrors.Count > 0)
        {
            return Result<TrustStatusResponse>.Failure(profileErrors.ToArray());
        }

        var manualReviewErrors = ValidateManualReviewRequest(request);
        if (manualReviewErrors.Count > 0)
        {
            return Result<TrustStatusResponse>.Failure(manualReviewErrors.ToArray());
        }

        var now = DateTimeOffset.UtcNow;
        var documentLast4 = NormalizeDocumentLast4(request.DocumentNumberLast4);
        var dataChanged = !string.Equals(verification.DeclaredLegalName, request.LegalName.Trim(), StringComparison.Ordinal)
            || !string.Equals(verification.DeclaredCountry, request.Country.Trim(), StringComparison.Ordinal)
            || !string.Equals(verification.DeclaredCity, request.City.Trim(), StringComparison.Ordinal)
            || !string.Equals(verification.DeclaredDocumentType, request.DocumentType.Trim(), StringComparison.Ordinal)
            || !string.Equals(verification.DeclaredDocumentNumberLast4, documentLast4, StringComparison.Ordinal);

        verification.DeclaredLegalName = request.LegalName.Trim();
        verification.DeclaredCountry = request.Country.Trim();
        verification.DeclaredCity = request.City.Trim();
        verification.DeclaredDocumentType = request.DocumentType.Trim();
        verification.DeclaredDocumentNumberLast4 = documentLast4;
        verification.ManualDeclarationAcceptedAt ??= now;
        verification.ManualReviewSubmittedAt = now;
        verification.ProfileSubmittedAt = now;
        verification.Status = UserVerificationStatus.InReview;
        verification.InReviewReason = "Guide profile is waiting for manual validation.";

        if (dataChanged)
        {
            verification.DeclaredDataReviewed = false;
            verification.ProfileCoherent = false;
            verification.EvidenceReviewStatus = ManualEvidenceReviewStatus.Pending;
            verification.EvidenceReviewedAt = null;
            verification.EvidenceReviewedByUserId = null;
            verification.EvidenceNotes = null;
        }

        await CreateReviewCaseIfMissingAsync(
            user.Id,
            AdminReviewCaseType.ProfileValidation,
            verification.InReviewReason,
            cancellationToken);

        await _trustRepository.SaveChangesAsync(cancellationToken);

        var roles = await _userAccountService.GetRolesAsync(user.Id, cancellationToken);
        return Result<TrustStatusResponse>.Success(TrustStatusMapper.Map(user, verification, roles.ToArray()));
    }

    private async Task<Result<ContactVerificationResponse>> RequestContactVerificationAsync(
        Guid userId,
        ContactVerificationChannel channel,
        CancellationToken cancellationToken)
    {
        if (channel == ContactVerificationChannel.Phone)
        {
            return Result<ContactVerificationResponse>.Failure("Phone validation is handled manually by WhatsApp.");
        }

        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<ContactVerificationResponse>.Failure("User was not found.");
        }

        var destination = channel == ContactVerificationChannel.Email
            ? user.Email
            : user.PhoneNumber;

        if (string.IsNullOrWhiteSpace(destination))
        {
            return Result<ContactVerificationResponse>.Failure($"{channel} is not configured for this user.");
        }

        if (channel == ContactVerificationChannel.Email && user.EmailConfirmed)
        {
            return Result<ContactVerificationResponse>.Failure("Email is already verified.");
        }

        if (channel == ContactVerificationChannel.Phone && user.PhoneNumberConfirmed)
        {
            return Result<ContactVerificationResponse>.Failure("Phone is already verified.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        var existingCodes = await _trustRepository.ListActiveContactCodesAsync(user.Id, channel, cancellationToken);

        foreach (var existingCode in existingCodes)
        {
            existingCode.IsUsed = true;
        }

        var code = GenerateCode();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(ContactCodeMinutes);
        _trustRepository.AddContactVerificationCode(new ContactVerificationCode
        {
            UserId = user.Id,
            Channel = channel,
            Destination = destination.Trim(),
            CodeHash = HashVerificationCode(user.Id, channel, code),
            ExpiresAt = expiresAt
        });

        SyncContactFlags(user, verification);
        UpdateDerivedStatus(user, verification);
        await _trustRepository.SaveChangesAsync(cancellationToken);

        return Result<ContactVerificationResponse>.Success(new ContactVerificationResponse(
            channel.ToString(),
            destination.Trim(),
            expiresAt,
            "Mock",
            code));
    }

    private async Task<Result<TrustStatusResponse>> ConfirmContactVerificationAsync(
        Guid userId,
        ContactVerificationChannel channel,
        ConfirmContactVerificationRequest request,
        CancellationToken cancellationToken)
    {
        if (channel == ContactVerificationChannel.Phone)
        {
            return Result<TrustStatusResponse>.Failure("Phone validation is handled manually by WhatsApp.");
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return Result<TrustStatusResponse>.Failure("Verification code is required.");
        }

        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<TrustStatusResponse>.Failure("User was not found.");
        }

        var verificationCode = await _trustRepository.GetLatestActiveContactCodeAsync(user.Id, channel, cancellationToken);
        if (verificationCode is null)
        {
            return Result<TrustStatusResponse>.Failure("No active verification code was found.");
        }

        verificationCode.Attempts++;
        if (verificationCode.Attempts > MaxContactCodeAttempts)
        {
            verificationCode.IsUsed = true;
            await _trustRepository.SaveChangesAsync(cancellationToken);
            return Result<TrustStatusResponse>.Failure("Verification code exceeded the maximum number of attempts.");
        }

        if (verificationCode.ExpiresAt < DateTimeOffset.UtcNow)
        {
            verificationCode.IsUsed = true;
            await _trustRepository.SaveChangesAsync(cancellationToken);
            return Result<TrustStatusResponse>.Failure("Verification code expired.");
        }

        var expectedHash = HashVerificationCode(user.Id, channel, request.Code.Trim());
        if (!string.Equals(expectedHash, verificationCode.CodeHash, StringComparison.Ordinal))
        {
            await _trustRepository.SaveChangesAsync(cancellationToken);
            return Result<TrustStatusResponse>.Failure("Verification code is invalid.");
        }

        var verification = await EnsureVerificationAsync(user, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        verificationCode.IsUsed = true;
        verificationCode.ConfirmedAt = now;

        Result updateResult;
        if (channel == ContactVerificationChannel.Email)
        {
            updateResult = await _userAccountService.SetEmailConfirmedAsync(user.Id, confirmed: true, cancellationToken);
            verification.EmailVerifiedAt ??= now;
            user = user with { EmailConfirmed = true };
        }
        else
        {
            updateResult = await _userAccountService.SetPhoneNumberConfirmedAsync(user.Id, confirmed: true, cancellationToken);
            verification.PhoneVerifiedAt ??= now;
            user = user with { PhoneNumberConfirmed = true };
        }

        if (!updateResult.Succeeded)
        {
            return Result<TrustStatusResponse>.Failure(updateResult.Errors.ToArray());
        }

        SyncContactFlags(user, verification);
        UpdateDerivedStatus(user, verification);
        await _trustRepository.SaveChangesAsync(cancellationToken);

        var roles = await _userAccountService.GetRolesAsync(user.Id, cancellationToken);
        return Result<TrustStatusResponse>.Success(TrustStatusMapper.Map(user, verification, roles.ToArray()));
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
        _trustRepository.AddUserVerification(verification);
        return verification;
    }

    private async Task CreateReviewCaseIfMissingAsync(
        Guid userId,
        AdminReviewCaseType type,
        string reason,
        CancellationToken cancellationToken)
    {
        var existingCase = await _trustRepository.GetOpenReviewCaseAsync(userId, type, cancellationToken);
        if (existingCase is not null)
        {
            existingCase.Reason = reason;
            return;
        }

        _trustRepository.AddAdminReviewCase(new AdminReviewCase
        {
            UserId = userId,
            Type = type,
            Reason = reason
        });
    }

    private static void SyncContactFlags(UserAccount user, UserVerification verification)
    {
        var now = DateTimeOffset.UtcNow;
        if (user.EmailConfirmed)
        {
            verification.EmailVerifiedAt ??= now;
        }

        _ = now;
    }

    private static void UpdateDerivedStatus(UserAccount user, UserVerification verification)
    {
        if (verification.Status == UserVerificationStatus.Suspended || verification.SuspendedAt.HasValue)
        {
            verification.Status = UserVerificationStatus.Suspended;
            return;
        }

        if (verification.Status is UserVerificationStatus.InReview or UserVerificationStatus.ProfileChangesRequested)
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

        verification.Status = IsContactVerified(user, verification)
            ? UserVerificationStatus.ContactVerified
            : UserVerificationStatus.Unverified;
    }

    private static bool IsContactVerified(UserAccount user, UserVerification verification)
    {
        return ManualReviewMapper.IsEmailConfirmed(user, verification)
            && ManualReviewMapper.IsPhoneContacted(verification);
    }

    private static IReadOnlyCollection<string> ValidateManualReviewRequest(SubmitManualGuideReviewRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.LegalName))
        {
            errors.Add("Legal name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Country))
        {
            errors.Add("Country is required.");
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            errors.Add("City is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DocumentType))
        {
            errors.Add("Document type is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DocumentNumberLast4))
        {
            errors.Add("Last document digits are required.");
        }
        else if (NormalizeDocumentLast4(request.DocumentNumberLast4).Length is < 2 or > 4)
        {
            errors.Add("Last document digits must contain 2 to 4 characters.");
        }

        if (!request.AcceptDeclaration)
        {
            errors.Add("Manual declaration must be accepted.");
        }

        return errors;
    }

    private static string NormalizeDocumentLast4(string value)
    {
        var normalized = new string(value.Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return normalized.Length <= 4 ? normalized : normalized[^4..];
    }

    private static IReadOnlyCollection<string> ValidateIdentityRequest(StartIdentityVerificationRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Country))
        {
            errors.Add("Country is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DocumentType))
        {
            errors.Add("Document type is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors.Add("Document number is required.");
        }

        if (!string.IsNullOrWhiteSpace(request.RequestedMockOutcome)
            && !Enum.TryParse<IdentityVerificationProviderStatus>(
                request.RequestedMockOutcome.Trim(),
                ignoreCase: true,
                out _))
        {
            errors.Add("Requested mock outcome must be Approved, Rejected or InReview.");
        }

        return errors;
    }

    private static IReadOnlyCollection<string> ValidateGuideProfile(GuideProfile profile)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(profile.Bio))
        {
            errors.Add("Guide profile bio is required.");
        }

        if (string.IsNullOrWhiteSpace(profile.City))
        {
            errors.Add("Guide profile city is required.");
        }

        if (string.IsNullOrWhiteSpace(profile.Country))
        {
            errors.Add("Guide profile country is required.");
        }

        if (string.IsNullOrWhiteSpace(profile.Specialties))
        {
            errors.Add("At least one guide specialty is required.");
        }

        if (string.IsNullOrWhiteSpace(profile.Languages))
        {
            errors.Add("At least one guide language is required.");
        }

        return errors;
    }

    private static IdentityVerificationAttemptStatus MapAttemptStatus(IdentityVerificationProviderStatus status)
    {
        return status switch
        {
            IdentityVerificationProviderStatus.Approved => IdentityVerificationAttemptStatus.Approved,
            IdentityVerificationProviderStatus.Rejected => IdentityVerificationAttemptStatus.Rejected,
            _ => IdentityVerificationAttemptStatus.InReview
        };
    }

    private static string GenerateCode()
    {
        return RandomNumberGenerator
            .GetInt32(0, 1_000_000)
            .ToString("D6", CultureInfo.InvariantCulture);
    }

    private static string HashVerificationCode(Guid userId, ContactVerificationChannel channel, string code)
    {
        var raw = $"{userId:N}:{channel}:{code.Trim()}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }

    private static string? GetLast4(string value)
    {
        var normalized = value.Trim();
        return normalized.Length <= 4 ? normalized : normalized[^4..];
    }
}
