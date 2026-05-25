using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Tourists;

public sealed class TouristProfile : AuditableEntity
{
    public Guid UserId { get; set; }
    public string? Country { get; set; }
    public string? PreferredLanguage { get; set; }
}
