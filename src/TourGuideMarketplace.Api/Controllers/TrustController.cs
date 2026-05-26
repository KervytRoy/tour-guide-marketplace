using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Common;
using TourGuideMarketplace.Contracts.Trust;

namespace TourGuideMarketplace.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/trust")]
public sealed class TrustController : ControllerBase
{
    private readonly ITrustService _trustService;

    public TrustController(ITrustService trustService)
    {
        _trustService = trustService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(TrustStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyStatus(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _trustService.GetMyStatusAsync(userId.Value, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("me/contact/email/request")]
    [ProducesResponseType(typeof(ContactVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestEmailVerification(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _trustService.RequestEmailVerificationAsync(userId.Value, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("me/contact/email/confirm")]
    [ProducesResponseType(typeof(TrustStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailVerification(
        ConfirmContactVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _trustService.ConfirmEmailVerificationAsync(userId.Value, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("me/contact/phone/request")]
    [ProducesResponseType(typeof(ContactVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPhoneVerification(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _trustService.RequestPhoneVerificationAsync(userId.Value, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("me/contact/phone/confirm")]
    [ProducesResponseType(typeof(TrustStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPhoneVerification(
        ConfirmContactVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _trustService.ConfirmPhoneVerificationAsync(userId.Value, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("me/identity/start")]
    [ProducesResponseType(typeof(IdentityVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartIdentityVerification(
        StartIdentityVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _trustService.StartIdentityVerificationAsync(userId.Value, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("me/rules/accept")]
    [ProducesResponseType(typeof(TrustStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptRules(
        AcceptTrustRulesRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _trustService.AcceptRulesAsync(userId.Value, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("me/guide-profile/submit-review")]
    [ProducesResponseType(typeof(TrustStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitGuideProfileReview(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponse(["Invalid access token."]));
        }

        var result = await _trustService.SubmitGuideProfileReviewAsync(userId.Value, cancellationToken);
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
