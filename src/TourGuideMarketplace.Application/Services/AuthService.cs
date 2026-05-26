using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Common.Security;
using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Auth;
using TourGuideMarketplace.Contracts.Security;
using TourGuideMarketplace.Domain.Tourists;
using TourGuideMarketplace.Domain.Trust;

namespace TourGuideMarketplace.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IGuideProfileRepository _guideProfileRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IApplicationTransactionRunner _transactionRunner;
    private readonly ITouristProfileRepository _touristProfileRepository;
    private readonly ITokenService _tokenService;
    private readonly ITrustRepository _trustRepository;
    private readonly IUserAccountService _userAccountService;

    public AuthService(
        IGuideProfileRepository guideProfileRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IApplicationTransactionRunner transactionRunner,
        ITouristProfileRepository touristProfileRepository,
        ITokenService tokenService,
        ITrustRepository trustRepository,
        IUserAccountService userAccountService)
    {
        _guideProfileRepository = guideProfileRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _transactionRunner = transactionRunner;
        _touristProfileRepository = touristProfileRepository;
        _tokenService = tokenService;
        _trustRepository = trustRepository;
        _userAccountService = userAccountService;
    }

    public Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        return _transactionRunner.ExecuteAsync(
            innerCancellationToken => RegisterCoreAsync(request, ipAddress, innerCancellationToken),
            cancellationToken);
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

        var user = await _userAccountService.FindByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        if (await _userAccountService.IsLockedOutAsync(user.Id, cancellationToken))
        {
            return Result<AuthResponse>.Failure("User is temporarily locked out.");
        }

        if (!await _userAccountService.CheckPasswordAsync(user.Id, request.Password, cancellationToken))
        {
            await _userAccountService.AccessFailedAsync(user.Id, cancellationToken);
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        await _userAccountService.ResetAccessFailedCountAsync(user.Id, cancellationToken);

        var response = await IssueTokensAsync(user, ipAddress, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
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

        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.FindByHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive || !storedToken.User.IsActive)
        {
            return Result<AuthResponse>.Failure("Invalid refresh token.");
        }

        var roles = await _userAccountService.GetRolesAsync(storedToken.User.Id, cancellationToken);
        var accessToken = _tokenService.CreateAccessToken(storedToken.User, roles);
        var refreshToken = _tokenService.CreateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);
        var now = DateTimeOffset.UtcNow;
        var refreshTokenExpiresAt = _tokenService.GetRefreshTokenExpiration(now);

        await _refreshTokenRepository.RevokeAsync(
            storedToken.Id,
            now,
            ipAddress,
            refreshTokenHash,
            cancellationToken);

        await _refreshTokenRepository.AddAsync(
            storedToken.User.Id,
            refreshTokenHash,
            refreshTokenExpiresAt,
            now,
            ipAddress,
            cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            storedToken.User.Id,
            storedToken.User.Email,
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

        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.FindActiveByUserIdAndHashAsync(
            userId,
            tokenHash,
            cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return Result.Failure("Invalid refresh token.");
        }

        await _refreshTokenRepository.RevokeAsync(
            storedToken.Id,
            DateTimeOffset.UtcNow,
            ipAddress,
            replacedByTokenHash: null,
            cancellationToken);

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<CurrentUserResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<CurrentUserResponse>.Failure("User was not found.");
        }

        var roles = await _userAccountService.GetRolesAsync(user.Id, cancellationToken);
        var guideProfile = roles.Contains(AppRoles.Guide, StringComparer.OrdinalIgnoreCase)
            ? await _guideProfileRepository.GetByUserIdAsync(user.Id, asTracking: false, cancellationToken)
            : null;
        var touristProfile = await _touristProfileRepository.GetByUserIdAsync(user.Id, asTracking: false, cancellationToken);

        return Result<CurrentUserResponse>.Success(new CurrentUserResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            roles.ToArray(),
            guideProfile?.Id,
            touristProfile?.Id));
    }

    private async Task<Result<AuthResponse>> RegisterCoreAsync(
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

        var existingUser = await _userAccountService.FindByEmailAsync(request.Email.Trim(), cancellationToken);
        if (existingUser is not null)
        {
            return Result<AuthResponse>.Failure("A user with this email already exists.");
        }

        var roleResult = await _userAccountService.EnsureRoleExistsAsync(role, cancellationToken);
        if (!roleResult.Succeeded)
        {
            return Result<AuthResponse>.Failure(roleResult.Errors.ToArray());
        }

        var createResult = await _userAccountService.CreateAsync(new CreateUserAccountRequest(
            request.Email.Trim(),
            request.FullName.Trim(),
            string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            request.Password), cancellationToken);

        if (!createResult.Succeeded || createResult.Value is null)
        {
            return Result<AuthResponse>.Failure(createResult.Errors.ToArray());
        }

        var user = createResult.Value;
        var addRoleResult = await _userAccountService.AddToRoleAsync(user.Id, role, cancellationToken);
        if (!addRoleResult.Succeeded)
        {
            return Result<AuthResponse>.Failure(addRoleResult.Errors.ToArray());
        }

        if (role == AppRoles.Tourist)
        {
            _touristProfileRepository.Add(new TouristProfile
            {
                UserId = user.Id
            });
        }

        _trustRepository.AddUserVerification(new UserVerification
        {
            UserId = user.Id
        });

        var response = await IssueTokensAsync(user, ipAddress, cancellationToken);
        await _trustRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(response);
    }

    private async Task<AuthResponse> IssueTokensAsync(
        UserAccount user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var roles = await _userAccountService.GetRolesAsync(user.Id, cancellationToken);
        var accessToken = _tokenService.CreateAccessToken(user, roles);
        var refreshToken = _tokenService.CreateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);
        var now = DateTimeOffset.UtcNow;
        var refreshTokenExpiresAt = _tokenService.GetRefreshTokenExpiration(now);

        await _refreshTokenRepository.AddAsync(
            user.Id,
            refreshTokenHash,
            refreshTokenExpiresAt,
            now,
            ipAddress,
            cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
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
