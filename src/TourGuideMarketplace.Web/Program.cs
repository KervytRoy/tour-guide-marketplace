using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TourGuideMarketplace.Web;
using TourGuideMarketplace.Web.Features.Guides;
using TourGuideMarketplace.Web.Features.Tourists;
using TourGuideMarketplace.Web.Features.Trust;
using TourGuideMarketplace.Web.Infrastructure.Api;
using TourGuideMarketplace.Web.Infrastructure.Auth;
using TourGuideMarketplace.Web.Infrastructure.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("API base URL was not configured.");

builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<CurrentUserState>();
builder.Services.AddScoped<GuidesApiClient>();
builder.Services.AddScoped<TouristsApiClient>();
builder.Services.AddScoped<TrustApiClient>();

var host = builder.Build();

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;

await host.RunAsync();
