using Microsoft.EntityFrameworkCore;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Guides;
using TourGuideMarketplace.Domain.Guides;

namespace TourGuideMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class GuideProfileRepository : IGuideProfileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public GuideProfileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GuideProfile?> GetByUserIdAsync(Guid userId, bool asTracking, CancellationToken cancellationToken)
    {
        var query = _dbContext.GuideProfiles.AsQueryable();
        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(guide => guide.UserId == userId, cancellationToken);
    }

    public Task<GuideProfile?> GetPublicByIdAsync(Guid guideProfileId, CancellationToken cancellationToken)
    {
        return _dbContext.GuideProfiles.AsNoTracking()
            .FirstOrDefaultAsync(guide => guide.Id == guideProfileId && guide.IsVerified, cancellationToken);
    }

    public async Task<IReadOnlyCollection<GuideProfile>> SearchPublicAsync(
        GuideSearchRequest request,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await ApplySearchFilters(_dbContext.GuideProfiles.AsNoTracking(), request)
            .OrderByDescending(guide => guide.IsVerified)
            .ThenByDescending(guide => guide.AverageRating)
            .ThenByDescending(guide => guide.ReviewsCount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountPublicAsync(GuideSearchRequest request, CancellationToken cancellationToken)
    {
        return ApplySearchFilters(_dbContext.GuideProfiles.AsNoTracking(), request)
            .CountAsync(cancellationToken);
    }

    public void Add(GuideProfile profile)
    {
        _dbContext.GuideProfiles.Add(profile);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<GuideProfile> ApplySearchFilters(
        IQueryable<GuideProfile> query,
        GuideSearchRequest request)
    {
        query = query.Where(guide => guide.IsVerified);

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City.Trim();
            query = query.Where(guide => guide.City == city);
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            var country = request.Country.Trim();
            query = query.Where(guide => guide.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(request.Specialty))
        {
            var specialty = request.Specialty.Trim();
            query = query.Where(guide => guide.Specialties.Contains(specialty));
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            var language = request.Language.Trim();
            query = query.Where(guide => guide.Languages.Contains(language));
        }

        if (request.MaxHourlyRate.HasValue)
        {
            query = query.Where(guide => guide.HourlyRate <= request.MaxHourlyRate.Value);
        }

        if (request.MinRating.HasValue)
        {
            query = query.Where(guide => guide.AverageRating >= request.MinRating.Value);
        }

        if (request.AvailableNow.HasValue)
        {
            query = query.Where(guide => guide.AvailableNow == request.AvailableNow.Value);
        }

        return query;
    }
}
