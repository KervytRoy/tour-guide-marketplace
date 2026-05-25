using TourGuideMarketplace.Contracts.Tourists;
using TourGuideMarketplace.Web.Infrastructure.Api;

namespace TourGuideMarketplace.Web.Features.Tourists;

public sealed class TouristsApiClient
{
    private readonly ApiClient _apiClient;

    public TouristsApiClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<TouristProfileResponse>> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<TouristProfileResponse>("api/tourists/me", cancellationToken: cancellationToken);
    }

    public Task<ApiResult<TouristProfileResponse>> UpsertMyProfileAsync(
        TouristProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PutAsync<TouristProfileRequest, TouristProfileResponse>(
            "api/tourists/me",
            request,
            cancellationToken: cancellationToken);
    }
}
