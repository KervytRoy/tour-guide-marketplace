using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Contracts.Auth;

namespace TourGuideMarketplace.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(Guid userId, LogoutRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task<Result<CurrentUserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
