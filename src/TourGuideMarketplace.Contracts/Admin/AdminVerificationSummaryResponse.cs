namespace TourGuideMarketplace.Contracts.Admin;

public sealed record AdminVerificationSummaryResponse(
    Guid UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    IReadOnlyCollection<string> Roles,
    string Status,
    bool EmailVerified,
    bool PhoneVerified,
    bool IdentityVerified,
    bool ProfileValidated,
    bool ManualReviewSubmitted,
    string EvidenceReviewStatus,
    string? IdentityProvider,
    string? LastAttemptStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
