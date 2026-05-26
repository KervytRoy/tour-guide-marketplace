using Microsoft.EntityFrameworkCore;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Domain.Tourists;

namespace TourGuideMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class TouristProfileRepository : ITouristProfileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TouristProfileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TouristProfile?> GetByUserIdAsync(Guid userId, bool asTracking, CancellationToken cancellationToken)
    {
        var query = _dbContext.TouristProfiles.AsQueryable();
        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(tourist => tourist.UserId == userId, cancellationToken);
    }

    public void Add(TouristProfile profile)
    {
        _dbContext.TouristProfiles.Add(profile);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
