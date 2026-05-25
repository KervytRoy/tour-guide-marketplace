namespace TourGuideMarketplace.Application.Guides;

public sealed record GuideProfileResponse(
    Guid Id,
    Guid UserId,
    string GuideName,
    string Bio,
    string City,
    string Country,
    decimal HourlyRate,
    string Currency,
    bool IsVerified,
    decimal AverageRating,
    int ReviewsCount,
    bool AvailableNow,
    decimal? Latitude,
    decimal? Longitude,
    IReadOnlyCollection<string> Specialties,
    IReadOnlyCollection<string> Languages);
