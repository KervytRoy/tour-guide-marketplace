using TourGuideMarketplace.Application.Common.Users;

namespace TourGuideMarketplace.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt,
        string? createdByIp,
        CancellationToken cancellationToken);

    Task<RefreshTokenRecord?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<RefreshTokenRecord?> FindActiveByUserIdAndHashAsync(
        Guid userId,
        string tokenHash,
        CancellationToken cancellationToken);

    Task RevokeAsync(
        Guid refreshTokenId,
        DateTimeOffset revokedAt,
        string? revokedByIp,
        string? replacedByTokenHash,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record RefreshTokenRecord(
    Guid Id,
    Guid UserId,
    string TokenHash,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    UserAccount User)
{
    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
}
