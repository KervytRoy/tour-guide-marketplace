namespace TourGuideMarketplace.Domain.Trust;

public enum ManualEvidenceReviewStatus
{
    Pending = 0,
    Received = 1,
    MatchesDeclaration = 2,
    Inconclusive = 3,
    Inconsistent = 4,
    Rejected = 5
}
