namespace TourGuideMarketplace.Contracts.Admin;

public sealed record AdminManualReviewUpdateRequest(
    string PhoneContactStatus,
    bool PhoneContacted,
    string? PhoneContactNotes,
    string EvidenceReviewStatus,
    bool EvidenceReceived,
    string? EvidenceNotes,
    bool DeclaredDataReviewed,
    bool ProfileCoherent,
    bool ReferencesReviewed,
    string ManualInterviewStatus,
    string ManualInterviewResult,
    string? ManualInterviewChannel,
    DateTimeOffset? ManualInterviewScheduledAt,
    DateTimeOffset? ManualInterviewCompletedAt,
    string? ManualInterviewReference,
    string? ManualInterviewNotes);
