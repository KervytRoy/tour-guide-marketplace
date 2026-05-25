using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Guides;

public sealed class GuideSpecialty : AuditableEntity
{
    public Guid GuideProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
}
