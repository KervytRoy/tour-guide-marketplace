namespace TourGuideMarketplace.Contracts.Trust;

public sealed record AcceptTrustRulesRequest(
    bool AcceptCodeOfConduct,
    bool AcceptSafetyRules);
