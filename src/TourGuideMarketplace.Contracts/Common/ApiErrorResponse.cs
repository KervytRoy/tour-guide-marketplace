namespace TourGuideMarketplace.Contracts.Common;

public sealed record ApiErrorResponse(IReadOnlyCollection<string> Errors);
