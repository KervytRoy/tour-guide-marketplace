namespace TourGuideMarketplace.Contracts.Trust;

public sealed record StartIdentityVerificationRequest(
    string Country,
    string DocumentType,
    string DocumentNumber,
    DateOnly? DateOfBirth,
    string? RequestedMockOutcome);
