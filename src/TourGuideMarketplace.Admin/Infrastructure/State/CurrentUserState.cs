using TourGuideMarketplace.Contracts.Auth;
using TourGuideMarketplace.Admin.Infrastructure.Api;
using TourGuideMarketplace.Admin.Infrastructure.Auth;

namespace TourGuideMarketplace.Admin.Infrastructure.State;

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

