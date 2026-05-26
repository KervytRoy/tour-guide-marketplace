using TourGuideMarketplace.Contracts.Guides;
using TourGuideMarketplace.Domain.Guides;

namespace TourGuideMarketplace.Application.Interfaces;

public interface IGuideProfileRepository
{
    Task<GuideProfile?> GetByUserIdAsync(Guid userId, bool asTracking, CancellationToken cancellationToken);
    Task<GuideProfile?> GetPublicByIdAsync(Guid guideProfileId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GuideProfile>> SearchPublicAsync(
        GuideSearchRequest request,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
    Task<int> CountPublicAsync(GuideSearchRequest request, CancellationToken cancellationToken);
    void Add(GuideProfile profile);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
