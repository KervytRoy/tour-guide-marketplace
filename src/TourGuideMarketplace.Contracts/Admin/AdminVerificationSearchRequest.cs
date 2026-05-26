namespace TourGuideMarketplace.Contracts.Admin;

public sealed record AdminVerificationSearchRequest(
    string? Status,
    int PageNumber,
    int PageSize);
