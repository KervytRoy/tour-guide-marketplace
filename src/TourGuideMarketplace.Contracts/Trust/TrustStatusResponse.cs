namespace TourGuideMarketplace.Contracts.Trust;

public sealed record TrustStatusResponse(
    Guid UserId,
    string Status,
    bool EmailVerified,
    bool PhoneVerified,
    bool IdentityVerified,
    bool ProfileValidated,
    bool CodeOfConductAccepted,
    bool SafetyRulesAccepted,
    bool ManualReviewSubmitted,
    bool ManualEvidenceReviewed,
    bool ManualInterviewCompleted,
    string? IdentityProvider,
    string? ExternalVerificationId,
    string? InReviewReason,
    string? SuspendedReason,
    DateTimeOffset? IdentityVerifiedAt,
    DateTimeOffset? ProfileValidatedAt,
    ManualReviewResponse? ManualReview,
    IReadOnlyCollection<TrustRequirementResponse> Requirements);
