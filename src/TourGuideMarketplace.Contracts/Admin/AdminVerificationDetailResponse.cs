using TourGuideMarketplace.Contracts.Trust;

namespace TourGuideMarketplace.Contracts.Admin;

public sealed record AdminVerificationDetailResponse(
    Guid UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    IReadOnlyCollection<string> Roles,
    AdminGuideProfileSnapshotResponse? GuideProfile,
    TrustStatusResponse TrustStatus,
    IReadOnlyCollection<AdminVerificationAttemptResponse> Attempts,
    ManualReviewResponse? ManualReview,
    IReadOnlyCollection<AdminReviewCaseResponse> ReviewCases);
