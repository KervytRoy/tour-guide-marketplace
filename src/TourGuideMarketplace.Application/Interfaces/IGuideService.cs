using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Guides;

namespace TourGuideMarketplace.Application.Interfaces;

public interface IGuideService
{
    Task<Result<GuideProfileResponse>> UpsertMyProfileAsync(Guid userId, GuideProfileRequest request, CancellationToken cancellationToken);
    Task<Result<GuideProfileResponse>> GetByIdAsync(Guid guideProfileId, CancellationToken cancellationToken);
    Task<Result<PagedResult<GuideProfileResponse>>> SearchAsync(GuideSearchRequest request, CancellationToken cancellationToken);
}
