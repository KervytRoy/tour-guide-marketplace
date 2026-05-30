using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TourGuideMarketplace.Contracts.Auth;
using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Admin.Infrastructure.Auth;

namespace TourGuideMarketplace.Admin.Infrastructure.Api;

public sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly JwtAuthenticationStateProvider _authenticationStateProvider;
    private readonly TokenStorage _tokenStorage;

    public ApiClient(
        HttpClient httpClient,
        JwtAuthenticationStateProvider authenticationStateProvider,
        TokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _tokenStorage = tokenStorage;
    }

    public Task<ApiResult<TResponse>> GetAsync<TResponse>(
        string path,
        bool authorized = true,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(() => new HttpRequestMessage(HttpMethod.Get, path), authorized, cancellationToken);
    }

    public Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        bool authorized = true,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(() => CreateJsonRequest(HttpMethod.Post, path, request), authorized, cancellationToken);
    }

    public Task<ApiResult<TResponse>> PostAsync<TResponse>(
        string path,
        bool authorized = true,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(() => new HttpRequestMessage(HttpMethod.Post, path), authorized, cancellationToken);
    }

    public Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        bool authorized = true,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(() => CreateJsonRequest(HttpMethod.Put, path, request), authorized, cancellationToken);
    }

    public Task<ApiResult> PostAsync<TRequest>(
        string path,
        TRequest request,
        bool authorized = true,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(() => CreateJsonRequest(HttpMethod.Post, path, request), authorized, cancellationToken);
    }

    private async Task<ApiResult<TResponse>> SendAsync<TResponse>(
        Func<HttpRequestMessage> requestFactory,
        bool authorized,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await SendWithRefreshAsync(requestFactory, authorized, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return ApiResult<TResponse>.Failure("No se pudo conectar con la API.");
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
                return value is null
                    ? ApiResult<TResponse>.Failure("The API returned an empty response.")
                    : ApiResult<TResponse>.Success(value);
            }

            return ApiResult<TResponse>.Failure(await ReadErrorsAsync(response, cancellationToken));
        }
    }

    private async Task<ApiResult> SendAsync(
        Func<HttpRequestMessage> requestFactory,
        bool authorized,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await SendWithRefreshAsync(requestFactory, authorized, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return ApiResult.Failure("No se pudo conectar con la API.");
        }

        using (response)
        {
            return response.IsSuccessStatusCode
                ? ApiResult.Success()
                : ApiResult.Failure(await ReadErrorsAsync(response, cancellationToken));
        }
    }

    private async Task<HttpResponseMessage> SendWithRefreshAsync(
        Func<HttpRequestMessage> requestFactory,
        bool authorized,
        CancellationToken cancellationToken)
    {
        using var request = requestFactory();
        await AttachAuthorizationHeaderAsync(request, authorized);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized || !authorized)
        {
            return response;
        }

        if (!await TryRefreshTokenAsync(cancellationToken))
        {
            return response;
        }

        response.Dispose();

        using var retryRequest = requestFactory();
        await AttachAuthorizationHeaderAsync(retryRequest, authorized);
        return await _httpClient.SendAsync(retryRequest, cancellationToken);
    }

    private async Task AttachAuthorizationHeaderAsync(HttpRequestMessage request, bool authorized)
    {
        if (!authorized)
        {
            return;
        }

        var auth = await _tokenStorage.GetAsync();
        if (!string.IsNullOrWhiteSpace(auth?.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        }
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        var auth = await _tokenStorage.GetAsync();
        if (string.IsNullOrWhiteSpace(auth?.RefreshToken))
        {
            return false;
        }

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(
                "api/auth/refresh-token",
                new RefreshTokenRequest(auth.RefreshToken),
                JsonOptions,
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return false;
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                await _tokenStorage.ClearAsync();
                _authenticationStateProvider.NotifyLoggedOut();
                return false;
            }

            var refreshed = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions, cancellationToken);
            if (refreshed is null)
            {
                return false;
            }

            await _tokenStorage.SaveAsync(refreshed);
            _authenticationStateProvider.NotifyAuthenticated(refreshed);
            return true;
        }
    }

    private static HttpRequestMessage CreateJsonRequest<TRequest>(HttpMethod method, string path, TRequest request)
    {
        return new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
    }

    private static async Task<IReadOnlyCollection<string>> ReadErrorsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var fallback = string.IsNullOrWhiteSpace(response.ReasonPhrase)
            ? $"Request failed with status {(int)response.StatusCode}."
            : response.ReasonPhrase;

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions, cancellationToken);
            return error?.Errors.Count > 0 ? error.Errors : [fallback];
        }
        catch (JsonException)
        {
            return [fallback];
        }
    }
}

