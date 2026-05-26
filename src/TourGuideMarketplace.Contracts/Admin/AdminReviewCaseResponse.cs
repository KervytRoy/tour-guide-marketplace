namespace TourGuideMarketplace.Contracts.Admin;

public sealed record AdminReviewCaseResponse(
    Guid Id,
    string Type,
    string Status,
    string Reason,
    string? Notes,
    string? Decision,
    Guid? ResolvedByUserId,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset CreatedAt);
