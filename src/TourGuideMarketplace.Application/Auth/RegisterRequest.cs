namespace TourGuideMarketplace.Application.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber,
    string Role);
