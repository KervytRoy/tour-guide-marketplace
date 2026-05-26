namespace TourGuideMarketplace.Contracts.Trust;

public sealed record TrustRequirementResponse(
    string Code,
    string Label,
    bool IsComplete,
    string? Action);
