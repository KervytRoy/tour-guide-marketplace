using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Interfaces;

namespace TourGuideMarketplace.Infrastructure.Persistence;

internal sealed class ApplicationTransactionRunner : IApplicationTransactionRunner
{
    private readonly ApplicationDbContext _dbContext;

    public ApplicationTransactionRunner(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var result = await operation(cancellationToken);

        if (result.Succeeded)
        {
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            await transaction.RollbackAsync(cancellationToken);
        }

        return result;
    }
}
