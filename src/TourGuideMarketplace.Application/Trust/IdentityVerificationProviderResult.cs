namespace TourGuideMarketplace.Application.Trust;

public sealed record IdentityVerificationProviderResult(
    string Provider,
    string ExternalVerificationId,
    IdentityVerificationProviderStatus Status,
    string? FailureReason,
    string RequestPayloadJson,
    string ResponsePayloadJson);
