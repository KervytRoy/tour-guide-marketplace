using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TourGuideMarketplace.Admin;
using TourGuideMarketplace.Admin.Features.Verifications;
using TourGuideMarketplace.Admin.Infrastructure.Api;
using TourGuideMarketplace.Admin.Infrastructure.Auth;
using TourGuideMarketplace.Admin.Infrastructure.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("API base URL was not configured.");

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<CurrentUserState>();
builder.Services.AddScoped<AdminVerificationsApiClient>();

await builder.Build().RunAsync();
