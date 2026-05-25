using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Common.Security;
using TourGuideMarketplace.Application.Guides;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Domain.Guides;
using TourGuideMarketplace.Infrastructure.Identity;
using TourGuideMarketplace.Infrastructure.Persistence;

namespace TourGuideMarketplace.Infrastructure.Services;

internal sealed class GuideService : IGuideService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public GuideService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<Result<GuideProfileResponse>> UpsertMyProfileAsync(
        Guid userId,
        GuideProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<GuideProfileResponse>.Failure("User was not found.");
        }

        if (!await _userManager.IsInRoleAsync(user, AppRoles.Guide))
        {
            return Result<GuideProfileResponse>.Failure("Only guide users can manage a guide profile.");
        }

        var validationErrors = ValidateProfileRequest(request);
        if (validationErrors.Count > 0)
        {
            return Result<GuideProfileResponse>.Failure(validationErrors.ToArray());
        }

        var profile = await _dbContext.GuideProfiles
            .Include(guide => guide.Specialties)
            .Include(guide => guide.Languages)
            .FirstOrDefaultAsync(guide => guide.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new GuideProfile { UserId = userId };
            _dbContext.GuideProfiles.Add(profile);
        }

        profile.Bio = request.Bio.Trim();
        profile.City = request.City.Trim();
        profile.Country = request.Country.Trim();
        profile.HourlyRate = request.HourlyRate;
        profile.Currency = request.Currency.Trim().ToUpperInvariant();
        profile.AvailableNow = request.AvailableNow;
        profile.Latitude = request.Latitude;
        profile.Longitude = request.Longitude;

        ReplaceSpecialties(profile, request.Specialties);
        ReplaceLanguages(profile, request.Languages);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<GuideProfileResponse>.Success(MapProfile(profile, user.FullName));
    }

    public async Task<Result<GuideProfileResponse>> GetByIdAsync(
        Guid guideProfileId,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.GuideProfiles
            .AsNoTracking()
            .Include(guide => guide.Specialties)
            .Include(guide => guide.Languages)
            .FirstOrDefaultAsync(guide => guide.Id == guideProfileId, cancellationToken);

        if (profile is null)
        {
            return Result<GuideProfileResponse>.Failure("Guide profile was not found.");
        }

        var guideName = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == profile.UserId)
            .Select(user => user.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        return Result<GuideProfileResponse>.Success(MapProfile(profile, guideName ?? string.Empty));
    }

    public async Task<Result<PagedResult<GuideProfileResponse>>> SearchAsync(
        GuideSearchRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.GuideProfiles
            .AsNoTracking()
            .AsQueryable();

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
            query = query.Where(guide => guide.Specialties.Any(item => item.Name == specialty));
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            var language = request.Language.Trim();
            query = query.Where(guide => guide.Languages.Any(item => item.Name == language));
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

        var totalCount = await query.CountAsync(cancellationToken);
        var profiles = await query
            .OrderByDescending(guide => guide.IsVerified)
            .ThenByDescending(guide => guide.AverageRating)
            .ThenByDescending(guide => guide.ReviewsCount)
            .Include(guide => guide.Specialties)
            .Include(guide => guide.Languages)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = profiles.Select(profile => profile.UserId).Distinct().ToArray();
        var guideNames = await _dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.FullName, cancellationToken);

        var items = profiles
            .Select(profile => MapProfile(
                profile,
                guideNames.GetValueOrDefault(profile.UserId, string.Empty)))
            .ToArray();

        return Result<PagedResult<GuideProfileResponse>>.Success(new PagedResult<GuideProfileResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount));
    }

    private static GuideProfileResponse MapProfile(GuideProfile profile, string guideName)
    {
        return new GuideProfileResponse(
            profile.Id,
            profile.UserId,
            guideName,
            profile.Bio,
            profile.City,
            profile.Country,
            profile.HourlyRate,
            profile.Currency,
            profile.IsVerified,
            profile.AverageRating,
            profile.ReviewsCount,
            profile.AvailableNow,
            profile.Latitude,
            profile.Longitude,
            profile.Specialties.Select(specialty => specialty.Name).Order().ToArray(),
            profile.Languages.Select(language => language.Name).Order().ToArray());
    }

    private static void ReplaceSpecialties(GuideProfile profile, IReadOnlyCollection<string> requestedSpecialties)
    {
        profile.Specialties.Clear();

        foreach (var specialty in NormalizeList(requestedSpecialties))
        {
            profile.Specialties.Add(new GuideSpecialty
            {
                GuideProfileId = profile.Id,
                Name = specialty
            });
        }
    }

    private static void ReplaceLanguages(GuideProfile profile, IReadOnlyCollection<string> requestedLanguages)
    {
        profile.Languages.Clear();

        foreach (var language in NormalizeList(requestedLanguages))
        {
            profile.Languages.Add(new GuideLanguage
            {
                GuideProfileId = profile.Id,
                Name = language
            });
        }
    }

    private static IReadOnlyCollection<string> NormalizeList(IReadOnlyCollection<string>? values)
    {
        return (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();
    }

    private static List<string> ValidateProfileRequest(GuideProfileRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Bio))
        {
            errors.Add("Bio is required.");
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            errors.Add("City is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Country))
        {
            errors.Add("Country is required.");
        }

        if (request.HourlyRate < 0)
        {
            errors.Add("Hourly rate cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Trim().Length != 3)
        {
            errors.Add("Currency must be a 3-letter ISO code.");
        }

        if (request.Latitude is < -90 or > 90)
        {
            errors.Add("Latitude must be between -90 and 90.");
        }

        if (request.Longitude is < -180 or > 180)
        {
            errors.Add("Longitude must be between -180 and 180.");
        }

        return errors;
    }
}
