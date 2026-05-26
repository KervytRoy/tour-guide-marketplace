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
        var emailVerified = user.EmailConfirmed || verification.EmailVerifiedAt.HasValue;
        var phoneVerified = user.PhoneNumberConfirmed || verification.PhoneVerifiedAt.HasValue;
        var identityVerified = verification.IdentityVerifiedAt.HasValue
            || verification.Status is UserVerificationStatus.IdentityVerified or UserVerificationStatus.ProfileValidated;
        var profileValidated = verification.ProfileValidatedAt.HasValue
            || verification.Status == UserVerificationStatus.ProfileValidated;
        var codeOfConductAccepted = verification.CodeOfConductAcceptedAt.HasValue;
        var safetyRulesAccepted = verification.SafetyRulesAcceptedAt.HasValue;

        var requirements = new List<TrustRequirementResponse>
        {
            new("email", "Email verificado", emailVerified, emailVerified ? null : "request-email-code"),
            new("phone", "Telefono verificado", phoneVerified, phoneVerified ? null : "request-phone-code"),
            new("identity", "Identidad verificada", identityVerified, identityVerified ? null : "start-identity-verification"),
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
                "Perfil de guia validado",
                profileValidated,
                profileValidated ? null : "submit-guide-profile-review"));
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
            verification.IdentityProvider,
            verification.ExternalVerificationId,
            verification.InReviewReason,
            verification.SuspendedReason,
            verification.IdentityVerifiedAt,
            verification.ProfileValidatedAt,
            requirements);
    }
}
