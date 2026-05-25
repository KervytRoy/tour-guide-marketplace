using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Guides;

public sealed class GuideProfile : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsVerified { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewsCount { get; set; }
    public bool AvailableNow { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public ICollection<GuideSpecialty> Specialties { get; set; } = [];
    public ICollection<GuideLanguage> Languages { get; set; } = [];
}
