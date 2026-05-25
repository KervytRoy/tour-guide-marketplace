namespace TourGuideMarketplace.Infrastructure.Auth;

internal sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);
