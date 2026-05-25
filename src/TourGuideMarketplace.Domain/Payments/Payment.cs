using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Payments;

public sealed class Payment : AuditableEntity
{
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal PlatformFeeAmount { get; set; }
    public string? Provider { get; set; }
    public string? ProviderPaymentId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
}
