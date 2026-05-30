using System.Text.Json;
using Microsoft.JSInterop;
using TourGuideMarketplace.Contracts.Auth;

namespace TourGuideMarketplace.Admin.Infrastructure.Auth;

public sealed class TokenStorage
{
    private const string StorageKey = "tourGuideMarketplace.admin.auth";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IJSRuntime _jsRuntime;

    public TokenStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<AuthResponse?> GetAsync()
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<AuthResponse>(json, JsonOptions);
    }

    public async ValueTask SaveAsync(AuthResponse auth)
    {
        var json = JsonSerializer.Serialize(auth, JsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async ValueTask ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }
}

