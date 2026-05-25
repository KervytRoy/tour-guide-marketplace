namespace TourGuideMarketplace.Api.Common;

public sealed record ApiErrorResponse(IReadOnlyCollection<string> Errors);
