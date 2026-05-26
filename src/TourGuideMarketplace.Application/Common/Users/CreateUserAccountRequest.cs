namespace TourGuideMarketplace.Application.Common.Users;

public sealed record CreateUserAccountRequest(
    string Email,
    string FullName,
    string? PhoneNumber,
    string Password);
