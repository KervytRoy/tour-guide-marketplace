using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TourGuideMarketplace.Contracts.Auth;

namespace TourGuideMarketplace.Web.Infrastructure.Auth;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState AnonymousState = new(new ClaimsPrincipal(new ClaimsIdentity()));
    private readonly TokenStorage _tokenStorage;

    public JwtAuthenticationStateProvider(TokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var auth = await _tokenStorage.GetAsync();
        if (auth is null || auth.AccessTokenExpiresAt <= DateTimeOffset.UtcNow)
        {
            return AnonymousState;
        }

        return CreateAuthenticationState(auth);
    }

    public void NotifyAuthenticated(AuthResponse auth)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticationState(auth)));
    }

    public void NotifyLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
    }

    private static AuthenticationState CreateAuthenticationState(AuthResponse auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, auth.UserId.ToString()),
            new(ClaimTypes.Email, auth.Email),
            new(ClaimTypes.Name, auth.FullName)
        };

        claims.AddRange(auth.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
