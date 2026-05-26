namespace TourGuideMarketplace.Application.Trust;

public sealed record IdentityVerificationProviderRequest(
    Guid UserId,
    string FullName,
    string Country,
    string DocumentType,
    string DocumentNumber,
    DateOnly? DateOfBirth,
    string? RequestedMockOutcome);
