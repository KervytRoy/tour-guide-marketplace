namespace TourGuideMarketplace.Contracts.Trust;

public sealed record SubmitManualGuideReviewRequest(
    string LegalName,
    string Country,
    string City,
    string DocumentType,
    string DocumentNumberLast4,
    bool AcceptDeclaration);
