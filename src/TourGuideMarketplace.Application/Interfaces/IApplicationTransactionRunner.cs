using TourGuideMarketplace.Application.Common.Models;

namespace TourGuideMarketplace.Application.Interfaces;

public interface IApplicationTransactionRunner
{
    Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken);
}
