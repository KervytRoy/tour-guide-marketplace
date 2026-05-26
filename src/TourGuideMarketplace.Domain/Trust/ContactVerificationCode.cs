using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Trust;

public sealed class ContactVerificationCode : AuditableEntity
{
    public Guid UserId { get; set; }
    public ContactVerificationChannel Channel { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int Attempts { get; set; }
    public bool IsUsed { get; set; }
    public string DeliveryProvider { get; set; } = "Mock";
}
