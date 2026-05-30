namespace TourGuideMarketplace.Domain.Trust;

public enum UserVerificationStatus
{
    Unverified = 0,
    ContactVerified = 1,
    IdentityVerified = 2,
    ProfileValidated = 3,
    InReview = 4,
    Suspended = 5,
    ProfileChangesRequested = 6
}
