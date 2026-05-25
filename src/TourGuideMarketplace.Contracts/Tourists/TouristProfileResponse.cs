namespace TourGuideMarketplace.Contracts.Tourists;

public sealed record TouristProfileResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    string? Country,
    string? PreferredLanguage);
