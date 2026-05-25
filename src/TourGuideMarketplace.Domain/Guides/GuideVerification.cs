using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Guides;

public sealed class GuideVerification : AuditableEntity
{
    public Guid GuideProfileId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public DateTimeOffset? ReviewedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? RejectionReason { get; set; }
}
