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
    string? IdentityProvider,
    string? ExternalVerificationId,
    string? InReviewReason,
    string? SuspendedReason,
    DateTimeOffset? IdentityVerifiedAt,
    DateTimeOffset? ProfileValidatedAt,
    IReadOnlyCollection<TrustRequirementResponse> Requirements);
