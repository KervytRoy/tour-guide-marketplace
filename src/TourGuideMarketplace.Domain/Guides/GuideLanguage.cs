using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Guides;

public sealed class GuideLanguage : AuditableEntity
{
    public Guid GuideProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
}
