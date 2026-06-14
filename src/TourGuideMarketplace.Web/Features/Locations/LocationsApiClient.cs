using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Contracts.Locations;
using TourGuideMarketplace.Web.Infrastructure.Api;

namespace TourGuideMarketplace.Web.Features.Locations;

public sealed class LocationsApiClient
{
    private readonly ApiClient _apiClient;

    public LocationsApiClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<IReadOnlyCollection<LocationSuggestionResponse>>> SuggestAsync(
        string? query,
        int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>();
        Add(parameters, "query", query);
        Add(parameters, "limit", limit.ToString());

        return _apiClient.GetAsync<IReadOnlyCollection<LocationSuggestionResponse>>(
            $"api/locations/suggest?{string.Join("&", parameters)}",
            authorized: false,
            cancellationToken: cancellationToken);
    }

    private static void Add(List<string> parameters, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
