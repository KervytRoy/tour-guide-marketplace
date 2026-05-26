using TourGuideMarketplace.Domain.Common;

namespace TourGuideMarketplace.Domain.Trust;

public sealed class UserVerification : AuditableEntity
{
    public Guid UserId { get; set; }
    public UserVerificationStatus Status { get; set; } = UserVerificationStatus.Unverified;
    public string? IdentityProvider { get; set; }
    public string? ExternalVerificationId { get; set; }
    public DateTimeOffset? EmailVerifiedAt { get; set; }
    public DateTimeOffset? PhoneVerifiedAt { get; set; }
    public DateTimeOffset? IdentityStartedAt { get; set; }
    public DateTimeOffset? IdentityVerifiedAt { get; set; }
    public DateTimeOffset? ProfileSubmittedAt { get; set; }
    public DateTimeOffset? ProfileValidatedAt { get; set; }
    public DateTimeOffset? CodeOfConductAcceptedAt { get; set; }
    public DateTimeOffset? SafetyRulesAcceptedAt { get; set; }
    public string? InReviewReason { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }
    public string? SuspendedReason { get; set; }
}
