using TourGuideMarketplace.Contracts.Locations;

namespace TourGuideMarketplace.Application.Interfaces;

public interface ILocationCatalogService
{
    Task<IReadOnlyCollection<LocationSuggestionResponse>> SuggestAsync(
        string? query,
        int limit,
        CancellationToken cancellationToken);
}
