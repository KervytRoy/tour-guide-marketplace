namespace TourGuideMarketplace.Contracts.Admin;

public sealed record AdminReviewDecisionRequest(
    string? Reason,
    string? Notes);
