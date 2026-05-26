using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Trust;

public sealed class AdminReviewCase : AuditableEntity
{
    public Guid UserId { get; set; }
    public AdminReviewCaseType Type { get; set; }
    public AdminReviewCaseStatus Status { get; set; } = AdminReviewCaseStatus.Open;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? Decision { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
}
