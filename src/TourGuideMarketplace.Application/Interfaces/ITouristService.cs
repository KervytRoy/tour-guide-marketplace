using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Contracts.Tourists;

namespace TourGuideMarketplace.Application.Interfaces;

public interface ITouristService
{
    Task<Result<TouristProfileResponse>> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<TouristProfileResponse>> UpsertMyProfileAsync(Guid userId, TouristProfileRequest request, CancellationToken cancellationToken);
}
