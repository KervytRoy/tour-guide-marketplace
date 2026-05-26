using Microsoft.EntityFrameworkCore;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Domain.Trust;

namespace TourGuideMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class TrustRepository : ITrustRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TrustRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserVerification?> GetUserVerificationAsync(
        Guid userId,
        bool asTracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.UserVerifications.AsQueryable();
        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(verification => verification.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserVerification>> ListUserVerificationsAsync(
        UserVerificationStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await ApplyStatusFilter(_dbContext.UserVerifications.AsNoTracking(), status)
            .OrderByDescending(verification => verification.Status == UserVerificationStatus.InReview)
            .ThenByDescending(verification => verification.UpdatedAt ?? verification.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountUserVerificationsAsync(
        UserVerificationStatus? status,
        CancellationToken cancellationToken)
    {
        return ApplyStatusFilter(_dbContext.UserVerifications.AsNoTracking(), status)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> ListUserIdsWithVerificationAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.UserVerifications
            .AsNoTracking()
            .Select(verification => verification.UserId)
            .ToArrayAsync(cancellationToken);
    }

    public void AddUserVerification(UserVerification verification)
    {
        _dbContext.UserVerifications.Add(verification);
    }

    public async Task<IReadOnlyCollection<ContactVerificationCode>> ListActiveContactCodesAsync(
        Guid userId,
        ContactVerificationChannel channel,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ContactVerificationCodes
            .Where(code => code.UserId == userId && code.Channel == channel && !code.IsUsed)
            .ToArrayAsync(cancellationToken);
    }

    public Task<ContactVerificationCode?> GetLatestActiveContactCodeAsync(
        Guid userId,
        ContactVerificationChannel channel,
        CancellationToken cancellationToken)
    {
        return _dbContext.ContactVerificationCodes
            .Where(code => code.UserId == userId && code.Channel == channel && !code.IsUsed)
            .OrderByDescending(code => code.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void AddContactVerificationCode(ContactVerificationCode code)
    {
        _dbContext.ContactVerificationCodes.Add(code);
    }

    public async Task<IReadOnlyCollection<IdentityVerificationAttempt>> ListIdentityAttemptsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        return await _dbContext.IdentityVerificationAttempts
            .AsNoTracking()
            .Where(attempt => userIds.Contains(attempt.UserId))
            .OrderByDescending(attempt => attempt.RequestedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<IdentityVerificationAttempt>> ListIdentityAttemptsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.IdentityVerificationAttempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId)
            .OrderByDescending(attempt => attempt.RequestedAt)
            .ToArrayAsync(cancellationToken);
    }

    public void AddIdentityVerificationAttempt(IdentityVerificationAttempt attempt)
    {
        _dbContext.IdentityVerificationAttempts.Add(attempt);
    }

    public Task<AdminReviewCase?> GetOpenReviewCaseAsync(
        Guid userId,
        AdminReviewCaseType type,
        CancellationToken cancellationToken)
    {
        return _dbContext.AdminReviewCases
            .FirstOrDefaultAsync(
                reviewCase => reviewCase.UserId == userId
                    && reviewCase.Type == type
                    && reviewCase.Status == AdminReviewCaseStatus.Open,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminReviewCase>> ListOpenReviewCasesAsync(
        Guid userId,
        AdminReviewCaseType type,
        CancellationToken cancellationToken)
    {
        return await _dbContext.AdminReviewCases
            .Where(reviewCase => reviewCase.UserId == userId
                && reviewCase.Type == type
                && reviewCase.Status == AdminReviewCaseStatus.Open)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AdminReviewCase>> ListReviewCasesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.AdminReviewCases
            .AsNoTracking()
            .Where(reviewCase => reviewCase.UserId == userId)
            .OrderByDescending(reviewCase => reviewCase.Status == AdminReviewCaseStatus.Open)
            .ThenByDescending(reviewCase => reviewCase.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public void AddAdminReviewCase(AdminReviewCase reviewCase)
    {
        _dbContext.AdminReviewCases.Add(reviewCase);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<UserVerification> ApplyStatusFilter(
        IQueryable<UserVerification> query,
        UserVerificationStatus? status)
    {
        return status.HasValue ? query.Where(verification => verification.Status == status.Value) : query;
    }
}
