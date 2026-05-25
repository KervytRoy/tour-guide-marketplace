using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Contracts.Security;
using TourGuideMarketplace.Contracts.Tourists;

namespace TourGuideMarketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.Tourist)]
[Route("api/tourists")]
public sealed class TouristsController : ControllerBase
{
    private readonly ITouristService _touristService;

    public TouristsController(ITouristService touristService)
    {
        _touristService = touristService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(TouristProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _touristService.GetMyProfileAsync(userId.Value, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(TouristProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertMyProfile(
        TouristProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _touristService.UpsertMyProfileAsync(userId.Value, request, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(Result<T> result)
    {
        return result.Succeeded
            ? Ok(result.Value)
            : BadRequest(new ApiErrorResponse(result.Errors));
    }

    private Guid? GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out var userId) ? userId : null;
    }
}
