using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Contracts.Guides;
using TourGuideMarketplace.Web.Infrastructure.Api;
using System.Globalization;

namespace TourGuideMarketplace.Web.Features.Guides;

public sealed class GuidesApiClient
{
    private readonly ApiClient _apiClient;

    public GuidesApiClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<PagedResult<GuideProfileResponse>>> SearchAsync(
        GuideSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<PagedResult<GuideProfileResponse>>(
            $"api/guides{BuildQueryString(request)}",
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<GuideProfileResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<GuideProfileResponse>($"api/guides/{id}", cancellationToken: cancellationToken);
    }

    public Task<ApiResult<GuideProfileResponse>> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<GuideProfileResponse>("api/guides/me", cancellationToken: cancellationToken);
    }

    public Task<ApiResult<GuideProfileResponse>> UpsertMyProfileAsync(
        GuideProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PutAsync<GuideProfileRequest, GuideProfileResponse>(
            "api/guides/me",
            request,
            cancellationToken: cancellationToken);
    }

    private static string BuildQueryString(GuideSearchRequest request)
    {
        var parameters = new List<string>();

        Add(parameters, "city", request.City);
        Add(parameters, "country", request.Country);
        Add(parameters, "specialty", request.Specialty);
        Add(parameters, "language", request.Language);
        Add(parameters, "maxHourlyRate", request.MaxHourlyRate?.ToString(CultureInfo.InvariantCulture));
        Add(parameters, "minRating", request.MinRating?.ToString(CultureInfo.InvariantCulture));
        Add(parameters, "availableNow", request.AvailableNow?.ToString().ToLowerInvariant());
        Add(parameters, "pageNumber", request.PageNumber.ToString());
        Add(parameters, "pageSize", request.PageSize.ToString());

        return parameters.Count == 0 ? string.Empty : $"?{string.Join("&", parameters)}";
    }

    private static void Add(List<string> parameters, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
