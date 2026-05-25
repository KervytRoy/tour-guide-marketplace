using TourGuideMarketplace.Contracts.Auth;
using TourGuideMarketplace.Web.Infrastructure.Api;

namespace TourGuideMarketplace.Web.Infrastructure.Auth;

public sealed class AuthSessionService
{
    private readonly ApiClient _apiClient;
    private readonly JwtAuthenticationStateProvider _authenticationStateProvider;
    private readonly TokenStorage _tokenStorage;

    public AuthSessionService(
        ApiClient apiClient,
        JwtAuthenticationStateProvider authenticationStateProvider,
        TokenStorage tokenStorage)
    {
        _apiClient = apiClient;
        _authenticationStateProvider = authenticationStateProvider;
        _tokenStorage = tokenStorage;
    }

    public async Task<ApiResult<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.PostAsync<LoginRequest, AuthResponse>(
            "api/auth/login",
            request,
            authorized: false,
            cancellationToken);

        if (result.Succeeded && result.Value is not null)
        {
            await _tokenStorage.SaveAsync(result.Value);
            _authenticationStateProvider.NotifyAuthenticated(result.Value);
        }

        return result;
    }

    public async Task<ApiResult<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.PostAsync<RegisterRequest, AuthResponse>(
            "api/auth/register",
            request,
            authorized: false,
            cancellationToken);

        if (result.Succeeded && result.Value is not null)
        {
            await _tokenStorage.SaveAsync(result.Value);
            _authenticationStateProvider.NotifyAuthenticated(result.Value);
        }

        return result;
    }

    public Task<ApiResult<CurrentUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<CurrentUserResponse>("api/auth/me", cancellationToken: cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var auth = await _tokenStorage.GetAsync();
        if (!string.IsNullOrWhiteSpace(auth?.RefreshToken))
        {
            await _apiClient.PostAsync(
                "api/auth/logout",
                new LogoutRequest(auth.RefreshToken),
                cancellationToken: cancellationToken);
        }

        await _tokenStorage.ClearAsync();
        _authenticationStateProvider.NotifyLoggedOut();
    }
}
