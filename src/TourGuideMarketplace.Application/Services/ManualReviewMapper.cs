using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Contracts.Trust;
using TourGuideMarketplace.Domain.Trust;

namespace TourGuideMarketplace.Application.Services;

internal static class ManualReviewMapper
{
    public static ManualReviewResponse Map(UserAccount user, UserVerification verification)
    {
        return new ManualReviewResponse(
            verification.DeclaredLegalName,
            verification.DeclaredCountry,
            verification.DeclaredCity,
            verification.DeclaredDocumentType,
            verification.DeclaredDocumentNumberLast4,
            verification.ManualDeclarationAcceptedAt.HasValue,
            verification.ManualReviewSubmittedAt,
            IsPhoneContacted(verification),
            verification.PhoneContactedAt,
            verification.PhoneContactNotes,
            verification.EvidenceReviewStatus.ToString(),
            verification.EvidenceReceivedAt,
            verification.EvidenceReviewedAt,
            verification.EvidenceNotes,
            verification.DeclaredDataReviewed,
            verification.ProfileCoherent,
            verification.ReferencesReviewed,
            verification.ManualInterviewStatus.ToString(),
            verification.ManualInterviewResult.ToString(),
            verification.ManualInterviewChannel,
            verification.ManualInterviewScheduledAt,
            verification.ManualInterviewCompletedAt,
            verification.ManualInterviewReference,
            verification.ManualInterviewNotes,
            CanApprove(user, verification));
    }

    public static bool CanApprove(UserAccount user, UserVerification verification)
    {
        return IsEmailConfirmed(user, verification)
            && IsPhoneContacted(verification)
            && verification.ManualReviewSubmittedAt.HasValue
            && verification.ManualDeclarationAcceptedAt.HasValue
            && HasDeclaredData(verification)
            && verification.DeclaredDataReviewed
            && verification.ProfileCoherent
            && verification.ReferencesReviewed
            && verification.EvidenceReviewStatus == ManualEvidenceReviewStatus.MatchesDeclaration
            && verification.ManualInterviewStatus == ManualInterviewStatus.Completed
            && verification.ManualInterviewResult == ManualInterviewResult.Passed
            && verification.CodeOfConductAcceptedAt.HasValue
            && verification.SafetyRulesAcceptedAt.HasValue;
    }

    public static bool IsEmailConfirmed(UserAccount user, UserVerification verification)
    {
        return user.EmailConfirmed || verification.EmailVerifiedAt.HasValue;
    }

    public static bool IsPhoneContacted(UserVerification verification)
    {
        return verification.PhoneContactedAt.HasValue;
    }

    public static bool HasDeclaredData(UserVerification verification)
    {
        return !string.IsNullOrWhiteSpace(verification.DeclaredLegalName)
            && !string.IsNullOrWhiteSpace(verification.DeclaredCountry)
            && !string.IsNullOrWhiteSpace(verification.DeclaredCity)
            && !string.IsNullOrWhiteSpace(verification.DeclaredDocumentType)
            && !string.IsNullOrWhiteSpace(verification.DeclaredDocumentNumberLast4);
    }
}
