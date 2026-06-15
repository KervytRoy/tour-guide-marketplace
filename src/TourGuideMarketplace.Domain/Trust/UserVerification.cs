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
    public string? DeclaredLegalName { get; set; }
    public string? DeclaredCountry { get; set; }
    public string? DeclaredCity { get; set; }
    public string? DeclaredDocumentType { get; set; }
    public string? DeclaredDocumentNumberLast4 { get; set; }
    public DateTimeOffset? ManualDeclarationAcceptedAt { get; set; }
    public DateTimeOffset? ManualReviewSubmittedAt { get; set; }
    public DateTimeOffset? ManualReviewCompletedAt { get; set; }
    public Guid? ManualReviewCompletedByUserId { get; set; }
    public DateTimeOffset? PhoneContactedAt { get; set; }
    public Guid? PhoneContactedByUserId { get; set; }
    public string? PhoneContactNotes { get; set; }
    public DateTimeOffset? EvidenceReceivedAt { get; set; }
    public ManualEvidenceReviewStatus EvidenceReviewStatus { get; set; } = ManualEvidenceReviewStatus.Pending;
    public DateTimeOffset? EvidenceReviewedAt { get; set; }
    public Guid? EvidenceReviewedByUserId { get; set; }
    public string? EvidenceNotes { get; set; }
    public bool DeclaredDataReviewed { get; set; }
    public bool ProfileCoherent { get; set; }
    public bool ReferencesReviewed { get; set; }
    public string? ManualInterviewChannel { get; set; }
    public DateTimeOffset? ManualInterviewScheduledAt { get; set; }
    public DateTimeOffset? ManualInterviewCompletedAt { get; set; }
    public string? ManualInterviewReference { get; set; }
    public ManualInterviewStatus ManualInterviewStatus { get; set; } = ManualInterviewStatus.Pending;
    public ManualInterviewResult ManualInterviewResult { get; set; } = ManualInterviewResult.None;
    public string? ManualInterviewNotes { get; set; }
    public Guid? ManualInterviewReviewedByUserId { get; set; }
    public string? InReviewReason { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }
    public string? SuspendedReason { get; set; }
}
