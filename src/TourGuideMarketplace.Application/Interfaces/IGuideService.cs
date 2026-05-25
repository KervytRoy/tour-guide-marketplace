using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Contracts.Guides;

namespace TourGuideMarketplace.Application.Interfaces;

public interface IGuideService
{
    Task<Result<GuideProfileResponse>> UpsertMyProfileAsync(Guid userId, GuideProfileRequest request, CancellationToken cancellationToken);
    Task<Result<GuideProfileResponse>> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<GuideProfileResponse>> GetByIdAsync(Guid guideProfileId, CancellationToken cancellationToken);
    Task<Result<PagedResult<GuideProfileResponse>>> SearchAsync(GuideSearchRequest request, CancellationToken cancellationToken);
}
