using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Admin;
using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Contracts.Security;

namespace TourGuideMarketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.Admin)]
[Route("api/admin/verifications")]
public sealed class AdminVerificationsController : ControllerBase
{
    private readonly IAdminTrustService _adminTrustService;

    public AdminVerificationsController(IAdminTrustService adminTrustService)
    {
        _adminTrustService = adminTrustService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminVerificationSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminTrustService.SearchAsync(
            new AdminVerificationSearchRequest(status, pageNumber, pageSize),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(AdminVerificationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDetail(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _adminTrustService.GetDetailAsync(userId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{userId:guid}/approve-profile")]
    [ProducesResponseType(typeof(AdminVerificationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveProfile(
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _adminTrustService.ApproveProfileAsync(adminUserId.Value, userId, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{userId:guid}/reject-profile")]
    [ProducesResponseType(typeof(AdminVerificationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectProfile(
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _adminTrustService.RejectProfileAsync(adminUserId.Value, userId, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{userId:guid}/suspend")]
    [ProducesResponseType(typeof(AdminVerificationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Suspend(
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _adminTrustService.SuspendAsync(adminUserId.Value, userId, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{userId:guid}/reactivate")]
    [ProducesResponseType(typeof(AdminVerificationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reactivate(
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        if (adminUserId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _adminTrustService.ReactivateAsync(adminUserId.Value, userId, request, cancellationToken);
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
