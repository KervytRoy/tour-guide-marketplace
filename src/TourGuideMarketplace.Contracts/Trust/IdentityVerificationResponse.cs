namespace TourGuideMarketplace.Contracts.Trust;

public sealed record IdentityVerificationResponse(
    string Provider,
    string ExternalVerificationId,
    string Status,
    string? FailureReason,
    TrustStatusResponse TrustStatus);
