using TourGuideMarketplace.Domain.Tourists;

namespace TourGuideMarketplace.Application.Interfaces;

public interface ITouristProfileRepository
{
    Task<TouristProfile?> GetByUserIdAsync(Guid userId, bool asTracking, CancellationToken cancellationToken);
    void Add(TouristProfile profile);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
