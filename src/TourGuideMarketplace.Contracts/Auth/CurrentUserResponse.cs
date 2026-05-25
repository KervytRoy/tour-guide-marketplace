namespace TourGuideMarketplace.Contracts.Auth;

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string FullName,
    string? PhoneNumber,
    IReadOnlyCollection<string> Roles,
    Guid? GuideProfileId,
    Guid? TouristProfileId);
