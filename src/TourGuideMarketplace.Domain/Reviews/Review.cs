using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Reviews;

public sealed class Review : AuditableEntity
{
    public Guid BookingId { get; set; }
    public Guid TouristProfileId { get; set; }
    public Guid GuideProfileId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
