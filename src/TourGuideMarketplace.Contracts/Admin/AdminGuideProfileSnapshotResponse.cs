namespace TourGuideMarketplace.Contracts.Admin;

public sealed record AdminGuideProfileSnapshotResponse(
    Guid Id,
    string Bio,
    string City,
    string Country,
    decimal HourlyRate,
    string Currency,
    bool IsVerified,
    bool AvailableNow,
    string Specialties,
    string Languages);
