namespace TourGuideMarketplace.Contracts.Locations;

public sealed record LocationSuggestionResponse(
    string City,
    string Country,
    string CountryCode,
    string? Region,
    decimal? Latitude,
    decimal? Longitude);
