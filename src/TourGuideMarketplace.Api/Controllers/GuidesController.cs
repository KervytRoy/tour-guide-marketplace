using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Common.Security;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Contracts.Guides;
using TourGuideMarketplace.Contracts.Security;

namespace TourGuideMarketplace.Api.Controllers;

[ApiController]
[Route("api/guides")]
public sealed class GuidesController : ControllerBase
{
    private readonly IGuideService _guideService;

    public GuidesController(IGuideService guideService)
    {
        _guideService = guideService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GuideProfileResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? city,
        [FromQuery] string? country,
        [FromQuery] string? specialty,
        [FromQuery] string? language,
        [FromQuery] decimal? maxHourlyRate,
        [FromQuery] decimal? minRating,
        [FromQuery] bool? availableNow,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new GuideSearchRequest(
            city,
            country,
            specialty,
            language,
            maxHourlyRate,
            minRating,
            availableNow,
            pageNumber,
            pageSize);

        var result = await _guideService.SearchAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GuideProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _guideService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize(Roles = AppRoles.Guide)]
    [HttpPost("me")]
    [HttpPut("me")]
    [ProducesResponseType(typeof(GuideProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertMyProfile(
        GuideProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _guideService.UpsertMyProfileAsync(userId.Value, request, cancellationToken);
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
