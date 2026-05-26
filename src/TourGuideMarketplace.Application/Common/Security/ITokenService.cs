using TourGuideMarketplace.Application.Common.Users;

namespace TourGuideMarketplace.Application.Common.Security;

public interface ITokenService
{
    AccessTokenResult CreateAccessToken(UserAccount user, IEnumerable<string> roles);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
    DateTimeOffset GetRefreshTokenExpiration(DateTimeOffset issuedAt);
}
