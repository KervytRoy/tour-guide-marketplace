using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TourGuideMarketplace.Application.Common.Security;
using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Application.Services;
using TourGuideMarketplace.Infrastructure.Auth;
using TourGuideMarketplace.Infrastructure.Identity;
using TourGuideMarketplace.Infrastructure.Persistence;
using TourGuideMarketplace.Infrastructure.Persistence.Repositories;
using TourGuideMarketplace.Infrastructure.Trust;

namespace TourGuideMarketplace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Application use cases.
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGuideService, GuideService>();
        services.AddScoped<ITouristService, TouristService>();
        services.AddScoped<ITrustService, TrustService>();
        services.AddScoped<IAdminTrustService, AdminTrustService>();

        // Infrastructure adapters for Application ports.
        services.AddScoped<IUserAccountService, IdentityUserAccountService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IApplicationTransactionRunner, ApplicationTransactionRunner>();
        services.AddScoped<IGuideProfileRepository, GuideProfileRepository>();
        services.AddScoped<ITouristProfileRepository, TouristProfileRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITrustRepository, TrustRepository>();
        services.AddScoped<IIdentityVerificationProvider, MockIdentityVerificationProvider>();

        return services;
    }
}
