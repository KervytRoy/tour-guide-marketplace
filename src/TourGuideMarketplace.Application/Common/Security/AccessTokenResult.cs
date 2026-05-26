namespace TourGuideMarketplace.Application.Common.Security;

public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);
