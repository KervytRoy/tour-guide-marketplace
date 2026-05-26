using TourGuideMarketplace.Domain.Trust;

namespace TourGuideMarketplace.Application.Interfaces;

public interface ITrustRepository
{
    Task<UserVerification?> GetUserVerificationAsync(Guid userId, bool asTracking, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserVerification>> ListUserVerificationsAsync(
        UserVerificationStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
    Task<int> CountUserVerificationsAsync(UserVerificationStatus? status, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Guid>> ListUserIdsWithVerificationAsync(CancellationToken cancellationToken);
    void AddUserVerification(UserVerification verification);

    Task<IReadOnlyCollection<ContactVerificationCode>> ListActiveContactCodesAsync(
        Guid userId,
        ContactVerificationChannel channel,
        CancellationToken cancellationToken);
    Task<ContactVerificationCode?> GetLatestActiveContactCodeAsync(
        Guid userId,
        ContactVerificationChannel channel,
        CancellationToken cancellationToken);
    void AddContactVerificationCode(ContactVerificationCode code);

    Task<IReadOnlyCollection<IdentityVerificationAttempt>> ListIdentityAttemptsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<IdentityVerificationAttempt>> ListIdentityAttemptsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);
    void AddIdentityVerificationAttempt(IdentityVerificationAttempt attempt);

    Task<AdminReviewCase?> GetOpenReviewCaseAsync(
        Guid userId,
        AdminReviewCaseType type,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminReviewCase>> ListOpenReviewCasesAsync(
        Guid userId,
        AdminReviewCaseType type,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminReviewCase>> ListReviewCasesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);
    void AddAdminReviewCase(AdminReviewCase reviewCase);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
