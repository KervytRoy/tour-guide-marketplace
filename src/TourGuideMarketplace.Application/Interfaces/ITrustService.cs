using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Contracts.Trust;

namespace TourGuideMarketplace.Application.Interfaces;

public interface ITrustService
{
    Task<Result<TrustStatusResponse>> GetMyStatusAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<ContactVerificationResponse>> RequestEmailVerificationAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<ContactVerificationResponse>> RequestPhoneVerificationAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<TrustStatusResponse>> ConfirmEmailVerificationAsync(
        Guid userId,
        ConfirmContactVerificationRequest request,
        CancellationToken cancellationToken);

    Task<Result<TrustStatusResponse>> ConfirmPhoneVerificationAsync(
        Guid userId,
        ConfirmContactVerificationRequest request,
        CancellationToken cancellationToken);

    Task<Result<IdentityVerificationResponse>> StartIdentityVerificationAsync(
        Guid userId,
        StartIdentityVerificationRequest request,
        CancellationToken cancellationToken);

    Task<Result<TrustStatusResponse>> AcceptRulesAsync(
        Guid userId,
        AcceptTrustRulesRequest request,
        CancellationToken cancellationToken);

    Task<Result<TrustStatusResponse>> SubmitGuideProfileReviewAsync(
        Guid userId,
        SubmitManualGuideReviewRequest request,
        CancellationToken cancellationToken);
}
