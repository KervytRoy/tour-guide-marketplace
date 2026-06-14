using Microsoft.AspNetCore.Mvc;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Locations;

namespace TourGuideMarketplace.Api.Controllers;

[ApiController]
[Route("api/locations")]
public sealed class LocationsController : ControllerBase
{
    private readonly ILocationCatalogService _locationCatalogService;

    public LocationsController(ILocationCatalogService locationCatalogService)
    {
        _locationCatalogService = locationCatalogService;
    }

    [HttpGet("suggest")]
    [ProducesResponseType(typeof(IReadOnlyCollection<LocationSuggestionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Suggest(
        [FromQuery] string? query,
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var suggestions = await _locationCatalogService.SuggestAsync(query, limit, cancellationToken);
        return Ok(suggestions);
    }
}
