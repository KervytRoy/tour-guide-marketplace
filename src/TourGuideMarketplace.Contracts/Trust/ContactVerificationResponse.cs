namespace TourGuideMarketplace.Contracts.Trust;

public sealed record ContactVerificationResponse(
    string Channel,
    string Destination,
    DateTimeOffset ExpiresAt,
    string DeliveryMethod,
    string? MockCode);
