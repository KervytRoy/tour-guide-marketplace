using TourGuideMarketplace.Contracts.Auth;
using TourGuideMarketplace.Web.Infrastructure.Api;
using TourGuideMarketplace.Web.Infrastructure.Auth;

namespace TourGuideMarketplace.Web.Infrastructure.State;

public sealed class CurrentUserState
{
    private readonly AuthSessionService _authSessionService;

    public CurrentUserResponse? User { get; private set; }

    public CurrentUserState(AuthSessionService authSessionService)
    {
        _authSessionService = authSessionService;
    }

    public async Task<ApiResult<CurrentUserResponse>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var result = await _authSessionService.GetCurrentUserAsync(cancellationToken);
        if (result.Succeeded)
        {
            User = result.Value;
        }

        return result;
    }

    public void Clear()
    {
        User = null;
    }
}
