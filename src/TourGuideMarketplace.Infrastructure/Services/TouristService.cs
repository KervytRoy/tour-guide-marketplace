using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Security;
using TourGuideMarketplace.Contracts.Tourists;
using TourGuideMarketplace.Domain.Tourists;
using TourGuideMarketplace.Infrastructure.Identity;
using TourGuideMarketplace.Infrastructure.Persistence;

namespace TourGuideMarketplace.Infrastructure.Services;

internal sealed class TouristService : ITouristService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public TouristService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<Result<TouristProfileResponse>> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<TouristProfileResponse>.Failure("User was not found.");
        }

        if (!await _userManager.IsInRoleAsync(user, AppRoles.Tourist))
        {
            return Result<TouristProfileResponse>.Failure("Only tourist users can access a tourist profile.");
        }

        var profile = await _dbContext.TouristProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(tourist => tourist.UserId == userId, cancellationToken);

        if (profile is null)
        {
            return Result<TouristProfileResponse>.Failure("Tourist profile was not found.");
        }

        return Result<TouristProfileResponse>.Success(MapProfile(profile, user.FullName));
    }

    public async Task<Result<TouristProfileResponse>> UpsertMyProfileAsync(
        Guid userId,
        TouristProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<TouristProfileResponse>.Failure("User was not found.");
        }

        if (!await _userManager.IsInRoleAsync(user, AppRoles.Tourist))
        {
            return Result<TouristProfileResponse>.Failure("Only tourist users can manage a tourist profile.");
        }

        var validationErrors = ValidateProfileRequest(request);
        if (validationErrors.Count > 0)
        {
            return Result<TouristProfileResponse>.Failure(validationErrors.ToArray());
        }

        var profile = await _dbContext.TouristProfiles
            .FirstOrDefaultAsync(tourist => tourist.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new TouristProfile { UserId = userId };
            _dbContext.TouristProfiles.Add(profile);
        }

        profile.Country = NormalizeOptional(request.Country);
        profile.PreferredLanguage = NormalizeOptional(request.PreferredLanguage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<TouristProfileResponse>.Success(MapProfile(profile, user.FullName));
    }

    private static TouristProfileResponse MapProfile(TouristProfile profile, string fullName)
    {
        return new TouristProfileResponse(
            profile.Id,
            profile.UserId,
            fullName,
            profile.Country,
            profile.PreferredLanguage);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static List<string> ValidateProfileRequest(TouristProfileRequest request)
    {
        var errors = new List<string>();

        if (request.Country?.Trim().Length > 120)
        {
            errors.Add("Country cannot exceed 120 characters.");
        }

        if (request.PreferredLanguage?.Trim().Length > 80)
        {
            errors.Add("Preferred language cannot exceed 80 characters.");
        }

        return errors;
    }
}
