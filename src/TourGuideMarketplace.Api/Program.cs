using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TourGuideMarketplace.Application.Common.Security;
using TourGuideMarketplace.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddDataProtection()
    .SetApplicationName("TourGuideMarketplace.Api")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));
builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration was not found.");

if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
{
    throw new InvalidOperationException("JWT secret must contain at least 32 characters.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy", service = "TourGuideMarketplace.Api" }));

app.Run();
