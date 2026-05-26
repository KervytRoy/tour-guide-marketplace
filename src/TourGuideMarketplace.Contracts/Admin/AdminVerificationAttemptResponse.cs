namespace TourGuideMarketplace.Contracts.Admin;

public sealed record AdminVerificationAttemptResponse(
    Guid Id,
    string Provider,
    string ExternalVerificationId,
    string Status,
    string Country,
    string DocumentType,
    string? DocumentNumberLast4,
    string? FailureReason,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt);
