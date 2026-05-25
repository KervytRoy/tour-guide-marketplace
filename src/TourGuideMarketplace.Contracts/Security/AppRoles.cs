namespace TourGuideMarketplace.Contracts.Security;

public static class AppRoles
{
    public const string Tourist = "Tourist";
    public const string Guide = "Guide";
    public const string Admin = "Admin";

    public static readonly string[] PublicRegistrationRoles = [Tourist, Guide];
}
