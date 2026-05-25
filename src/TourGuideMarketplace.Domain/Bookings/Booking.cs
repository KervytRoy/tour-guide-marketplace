using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Bookings;

public sealed class Booking : AuditableEntity
{
    public Guid TouristProfileId { get; set; }
    public Guid GuideProfileId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string MeetingPoint { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? TouristNotes { get; set; }
    public string? CancellationReason { get; set; }
}
