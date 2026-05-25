namespace TourGuideMarketplace.Contracts.Tourists;

public sealed record TouristProfileRequest(
    string? Country,
    string? PreferredLanguage);
