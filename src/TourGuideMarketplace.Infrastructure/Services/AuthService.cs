using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Common.Security;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Auth;
using TourGuideMarketplace.Contracts.Security;
using TourGuideMarketplace.Domain.Tourists;
using TourGuideMarketplace.Infrastructure.Auth;
using TourGuideMarketplace.Infrastructure.Identity;
using TourGuideMarketplace.Infrastructure.Persistence;

namespace TourGuideMarketplace.Infrastructure.Services;

internal sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(
        ApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateRegisterRequest(request);
        if (validationErrors.Count > 0)
        {
            return Result<AuthResponse>.Failure(validationErrors.ToArray());
        }

        var role = NormalizeRegistrationRole(request.Role);
        if (role is null)
        {
            return Result<AuthResponse>.Failure("Role must be Tourist or Guide.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Result<AuthResponse>.Failure("A user with this email already exists.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (!await _roleManager.RoleExistsAsync(role))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (!roleResult.Succeeded)
            {
                return Result<AuthResponse>.Failure(roleResult.Errors.Select(error => error.Description).ToArray());
            }
        }

        var user = new ApplicationUser
        {
            Email = request.Email.Trim(),
            UserName = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim()
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return Result<AuthResponse>.Failure(createResult.Errors.Select(error => error.Description).ToArray());
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            return Result<AuthResponse>.Failure(addRoleResult.Errors.Select(error => error.Description).ToArray());
        }

        if (role == AppRoles.Tourist)
        {
            _dbContext.TouristProfiles.Add(new TouristProfile
            {
                UserId = user.Id
            });
        }

        var response = await IssueTokensAsync(user, ipAddress, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("Email and password are required.");
        }

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
        {
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result<AuthResponse>.Failure("User is temporarily locked out.");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var response = await IssueTokensAsync(user, ipAddress, cancellationToken);
        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<AuthResponse>.Failure("Refresh token is required.");
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive || !storedToken.User.IsActive)
        {
            return Result<AuthResponse>.Failure("Invalid refresh token.");
        }

        var roles = await _userManager.GetRolesAsync(storedToken.User);
        var accessToken = _jwtTokenService.CreateAccessToken(storedToken.User, roles);
        var refreshToken = _jwtTokenService.CreateRefreshToken();
        var refreshTokenHash = _jwtTokenService.HashRefreshToken(refreshToken);
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.RevokedByIp = ipAddress;
        storedToken.ReplacedByTokenHash = refreshTokenHash;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = storedToken.UserId,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = ipAddress
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            storedToken.User.Id,
            storedToken.User.Email ?? string.Empty,
            storedToken.User.FullName,
            roles.ToArray(),
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken,
            refreshTokenExpiresAt));
    }

    public async Task<Result> LogoutAsync(
        Guid userId,
        LogoutRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Failure("Refresh token is required.");
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                token => token.UserId == userId && token.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return Result.Failure("Invalid refresh token.");
        }

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.RevokedByIp = ipAddress;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<CurrentUserResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<CurrentUserResponse>.Failure("User was not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var guideProfileId = await _dbContext.GuideProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var touristProfileId = await _dbContext.TouristProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return Result<CurrentUserResponse>.Success(new CurrentUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            user.PhoneNumber,
            roles.ToArray(),
            guideProfileId,
            touristProfileId));
    }

    private async Task<AuthResponse> IssueTokensAsync(
        ApplicationUser user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.CreateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.CreateRefreshToken();
        var refreshTokenHash = _jwtTokenService.HashRefreshToken(refreshToken);
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = ipAddress
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            roles.ToArray(),
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken,
            refreshTokenExpiresAt);
    }

    private static List<string> ValidateRegisterRequest(RegisterRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            errors.Add("Password must contain at least 8 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors.Add("Full name is required.");
        }

        return errors;
    }

    private static string? NormalizeRegistrationRole(string role)
    {
        return AppRoles.PublicRegistrationRoles.FirstOrDefault(
            allowedRole => string.Equals(allowedRole, role?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
