namespace TourGuideMarketplace.Contracts.Guides;

public sealed record GuideSearchRequest(
    string? City,
    string? Country,
    string? Specialty,
    string? Language,
    decimal? MaxHourlyRate,
    decimal? MinRating,
    bool? AvailableNow,
    int PageNumber = 1,
    int PageSize = 20);
