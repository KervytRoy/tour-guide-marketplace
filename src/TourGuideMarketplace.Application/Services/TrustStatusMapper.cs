using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Contracts.Security;
using TourGuideMarketplace.Contracts.Trust;
using TourGuideMarketplace.Domain.Trust;

namespace TourGuideMarketplace.Application.Services;

internal static class TrustStatusMapper
{
    public static TrustStatusResponse Map(
        UserAccount user,
        UserVerification verification,
        IReadOnlyCollection<string> roles)
    {
        var emailVerified = ManualReviewMapper.IsEmailConfirmed(user, verification);
        var phoneVerified = ManualReviewMapper.IsPhoneContacted(verification);
        var identityVerified = verification.IdentityVerifiedAt.HasValue
            || verification.Status is UserVerificationStatus.IdentityVerified or UserVerificationStatus.ProfileValidated;
        var profileValidated = verification.ProfileValidatedAt.HasValue
            || verification.Status == UserVerificationStatus.ProfileValidated;
        var codeOfConductAccepted = verification.CodeOfConductAcceptedAt.HasValue;
        var safetyRulesAccepted = verification.SafetyRulesAcceptedAt.HasValue;
        var manualEvidenceReviewed = verification.EvidenceReviewStatus == ManualEvidenceReviewStatus.MatchesDeclaration;
        var manualInterviewCompleted = verification.ManualInterviewStatus == ManualInterviewStatus.Completed
            && verification.ManualInterviewResult == ManualInterviewResult.Passed;

        var requirements = new List<TrustRequirementResponse>
        {
            new("email", "Email confirmado", emailVerified, emailVerified ? null : "request-email-code"),
            new("phone", "Telefono contactado por WhatsApp", phoneVerified, phoneVerified ? null : "manual-whatsapp-contact"),
            new("manual-evidence", "Documento y declaracion jurada revisados", manualEvidenceReviewed, manualEvidenceReviewed ? null : "manual-evidence-review"),
            new("manual-interview", "Entrevista manual completada", manualInterviewCompleted, manualInterviewCompleted ? null : "manual-interview"),
            new(
                "rules",
                "Reglas de seguridad aceptadas",
                codeOfConductAccepted && safetyRulesAccepted,
                codeOfConductAccepted && safetyRulesAccepted ? null : "accept-trust-rules")
        };

        if (roles.Contains(AppRoles.Guide, StringComparer.OrdinalIgnoreCase))
        {
            requirements.Add(new(
                "guide-profile",
                "Perfil de guia completo",
                profileValidated || verification.ManualReviewSubmittedAt.HasValue,
                profileValidated || verification.ManualReviewSubmittedAt.HasValue ? null : "submit-guide-profile-review"));
        }

        return new TrustStatusResponse(
            user.Id,
            verification.Status.ToString(),
            emailVerified,
            phoneVerified,
            identityVerified,
            profileValidated,
            codeOfConductAccepted,
            safetyRulesAccepted,
            verification.ManualReviewSubmittedAt.HasValue,
            manualEvidenceReviewed,
            manualInterviewCompleted,
            verification.IdentityProvider,
            verification.ExternalVerificationId,
            verification.InReviewReason,
            verification.SuspendedReason,
            verification.IdentityVerifiedAt,
            verification.ProfileValidatedAt,
            ManualReviewMapper.Map(user, verification),
            requirements);
    }
}
