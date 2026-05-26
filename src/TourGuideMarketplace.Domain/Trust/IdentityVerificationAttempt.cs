using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Trust;

public sealed class IdentityVerificationAttempt : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ExternalVerificationId { get; set; } = string.Empty;
    public IdentityVerificationAttemptStatus Status { get; set; }
    public string Country { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumberLast4 { get; set; }
    public string? FailureReason { get; set; }
    public string RequestPayloadJson { get; set; } = string.Empty;
    public string ResponsePayloadJson { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
