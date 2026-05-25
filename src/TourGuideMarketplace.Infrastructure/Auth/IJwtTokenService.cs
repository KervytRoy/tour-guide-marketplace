using TourGuideMarketplace.Infrastructure.Identity;

namespace TourGuideMarketplace.Infrastructure.Auth;

internal interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
