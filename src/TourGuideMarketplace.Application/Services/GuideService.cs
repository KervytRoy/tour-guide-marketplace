using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Contracts.Guides;
using TourGuideMarketplace.Contracts.Security;
using TourGuideMarketplace.Domain.Guides;

namespace TourGuideMarketplace.Application.Services;

public sealed class GuideService : IGuideService
{
    private readonly IGuideProfileRepository _guideProfileRepository;
    private readonly IUserAccountService _userAccountService;

    public GuideService(IGuideProfileRepository guideProfileRepository, IUserAccountService userAccountService)
    {
        _guideProfileRepository = guideProfileRepository;
        _userAccountService = userAccountService;
    }

    public async Task<Result<GuideProfileResponse>> UpsertMyProfileAsync(
        Guid userId,
        GuideProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<GuideProfileResponse>.Failure("User was not found.");
        }

        if (!await _userAccountService.IsInRoleAsync(user.Id, AppRoles.Guide, cancellationToken))
        {
            return Result<GuideProfileResponse>.Failure("Only guide users can manage a guide profile.");
        }

        var validationErrors = ValidateProfileRequest(request);
        if (validationErrors.Count > 0)
        {
            return Result<GuideProfileResponse>.Failure(validationErrors.ToArray());
        }

        var profile = await _guideProfileRepository.GetByUserIdAsync(userId, asTracking: true, cancellationToken);

        if (profile is null)
        {
            profile = new GuideProfile { UserId = userId };
            _guideProfileRepository.Add(profile);
        }

        profile.Bio = request.Bio.Trim();
        profile.City = request.City.Trim();
        profile.Country = request.Country.Trim();
        profile.HourlyRate = request.HourlyRate;
        profile.Currency = request.Currency.Trim().ToUpperInvariant();
        profile.AvailableNow = request.AvailableNow;
        profile.Latitude = request.Latitude;
        profile.Longitude = request.Longitude;
        profile.Specialties = NormalizeText(request.Specialties);
        profile.Languages = NormalizeText(request.Languages);

        await _guideProfileRepository.SaveChangesAsync(cancellationToken);

        return Result<GuideProfileResponse>.Success(MapProfile(profile, user.FullName));
    }

    public async Task<Result<GuideProfileResponse>> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<GuideProfileResponse>.Failure("User was not found.");
        }

        if (!await _userAccountService.IsInRoleAsync(user.Id, AppRoles.Guide, cancellationToken))
        {
            return Result<GuideProfileResponse>.Failure("Only guide users can access a guide profile.");
        }

        var profile = await _guideProfileRepository.GetByUserIdAsync(userId, asTracking: false, cancellationToken);

        if (profile is null)
        {
            return Result<GuideProfileResponse>.Failure("Guide profile was not found.");
        }

        return Result<GuideProfileResponse>.Success(MapProfile(profile, user.FullName));
    }

    public async Task<Result<GuideProfileResponse>> GetByIdAsync(
        Guid guideProfileId,
        CancellationToken cancellationToken)
    {
        var profile = await _guideProfileRepository.GetPublicByIdAsync(guideProfileId, cancellationToken);

        if (profile is null)
        {
            return Result<GuideProfileResponse>.Failure("Guide profile was not found.");
        }

        var user = await _userAccountService.FindByIdAsync(profile.UserId, cancellationToken);

        return Result<GuideProfileResponse>.Success(MapProfile(profile, user?.FullName ?? string.Empty));
    }

    public async Task<Result<PagedResult<GuideProfileResponse>>> SearchAsync(
        GuideSearchRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var totalCount = await _guideProfileRepository.CountPublicAsync(request, cancellationToken);
        var profiles = await _guideProfileRepository.SearchPublicAsync(
            request,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = new List<GuideProfileResponse>();
        foreach (var profile in profiles)
        {
            var user = await _userAccountService.FindByIdAsync(profile.UserId, cancellationToken);
            items.Add(MapProfile(profile, user?.FullName ?? string.Empty));
        }

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
            profile.Specialties,
            profile.Languages);
    }

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
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

        if (request.Specialties?.Trim().Length > 2000)
        {
            errors.Add("Specialties cannot exceed 2000 characters.");
        }

        if (request.Languages?.Trim().Length > 2000)
        {
            errors.Add("Languages cannot exceed 2000 characters.");
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
