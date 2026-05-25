namespace TourGuideMarketplace.Contracts.Guides;

public sealed record GuideProfileRequest(
    string Bio,
    string City,
    string Country,
    decimal HourlyRate,
    string Currency,
    bool AvailableNow,
    decimal? Latitude,
    decimal? Longitude,
    IReadOnlyCollection<string> Specialties,
    IReadOnlyCollection<string> Languages);
