using Microsoft.EntityFrameworkCore;
using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Infrastructure.Identity;

namespace TourGuideMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt,
        string? createdByIp,
        CancellationToken cancellationToken)
    {
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt,
            CreatedByIp = createdByIp
        });

        return Task.CompletedTask;
    }

    public async Task<RefreshTokenRecord?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var token = await _dbContext.RefreshTokens
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);

        return token is null ? null : Map(token);
    }

    public async Task<RefreshTokenRecord?> FindActiveByUserIdAndHashAsync(
        Guid userId,
        string tokenHash,
        CancellationToken cancellationToken)
    {
        var token = await _dbContext.RefreshTokens
            .Include(item => item.User)
            .FirstOrDefaultAsync(
                item => item.UserId == userId && item.TokenHash == tokenHash,
                cancellationToken);

        return token is null ? null : Map(token);
    }

    public async Task RevokeAsync(
        Guid refreshTokenId,
        DateTimeOffset revokedAt,
        string? revokedByIp,
        string? replacedByTokenHash,
        CancellationToken cancellationToken)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(item => item.Id == refreshTokenId, cancellationToken);

        if (token is null)
        {
            return;
        }

        token.RevokedAt = revokedAt;
        token.RevokedByIp = revokedByIp;
        token.ReplacedByTokenHash = replacedByTokenHash;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static RefreshTokenRecord Map(RefreshToken token)
    {
        return new RefreshTokenRecord(
            token.Id,
            token.UserId,
            token.TokenHash,
            token.ExpiresAt,
            token.RevokedAt,
            new UserAccount(
                token.User.Id,
                token.User.Email ?? string.Empty,
                token.User.FullName,
                token.User.PhoneNumber,
                token.User.EmailConfirmed,
                token.User.PhoneNumberConfirmed,
                token.User.IsActive));
    }
}
