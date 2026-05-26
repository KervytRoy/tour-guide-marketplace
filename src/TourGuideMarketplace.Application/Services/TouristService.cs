using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Security;
using TourGuideMarketplace.Contracts.Tourists;
using TourGuideMarketplace.Domain.Tourists;

namespace TourGuideMarketplace.Application.Services;

public sealed class TouristService : ITouristService
{
    private readonly ITouristProfileRepository _touristProfileRepository;
    private readonly IUserAccountService _userAccountService;

    public TouristService(ITouristProfileRepository touristProfileRepository, IUserAccountService userAccountService)
    {
        _touristProfileRepository = touristProfileRepository;
        _userAccountService = userAccountService;
    }

    public async Task<Result<TouristProfileResponse>> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<TouristProfileResponse>.Failure("User was not found.");
        }

        if (!await _userAccountService.IsInRoleAsync(user.Id, AppRoles.Tourist, cancellationToken))
        {
            return Result<TouristProfileResponse>.Failure("Only tourist users can access a tourist profile.");
        }

        var profile = await _touristProfileRepository.GetByUserIdAsync(userId, asTracking: false, cancellationToken);

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
        var user = await _userAccountService.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<TouristProfileResponse>.Failure("User was not found.");
        }

        if (!await _userAccountService.IsInRoleAsync(user.Id, AppRoles.Tourist, cancellationToken))
        {
            return Result<TouristProfileResponse>.Failure("Only tourist users can manage a tourist profile.");
        }

        var validationErrors = ValidateProfileRequest(request);
        if (validationErrors.Count > 0)
        {
            return Result<TouristProfileResponse>.Failure(validationErrors.ToArray());
        }

        var profile = await _touristProfileRepository.GetByUserIdAsync(userId, asTracking: true, cancellationToken);

        if (profile is null)
        {
            profile = new TouristProfile { UserId = userId };
            _touristProfileRepository.Add(profile);
        }

        profile.Country = NormalizeOptional(request.Country);
        profile.PreferredLanguage = NormalizeOptional(request.PreferredLanguage);

        await _touristProfileRepository.SaveChangesAsync(cancellationToken);

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
