namespace TourGuideMarketplace.Application.Common.Users;

public sealed record UserAccount(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    bool IsActive);
