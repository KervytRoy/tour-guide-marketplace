using TourGuideMarketplace.Contracts.Trust;
using TourGuideMarketplace.Web.Infrastructure.Api;

namespace TourGuideMarketplace.Web.Features.Trust;

public sealed class TrustApiClient
{
    private readonly ApiClient _apiClient;

    public TrustApiClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<TrustStatusResponse>> GetMyStatusAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<TrustStatusResponse>("api/trust/me", cancellationToken: cancellationToken);
    }

    public Task<ApiResult<ContactVerificationResponse>> RequestEmailVerificationAsync(
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<ContactVerificationResponse>(
            "api/trust/me/contact/email/request",
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<TrustStatusResponse>> ConfirmEmailVerificationAsync(
        ConfirmContactVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<ConfirmContactVerificationRequest, TrustStatusResponse>(
            "api/trust/me/contact/email/confirm",
            request,
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<ContactVerificationResponse>> RequestPhoneVerificationAsync(
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<ContactVerificationResponse>(
            "api/trust/me/contact/phone/request",
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<TrustStatusResponse>> ConfirmPhoneVerificationAsync(
        ConfirmContactVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<ConfirmContactVerificationRequest, TrustStatusResponse>(
            "api/trust/me/contact/phone/confirm",
            request,
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<IdentityVerificationResponse>> StartIdentityVerificationAsync(
        StartIdentityVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<StartIdentityVerificationRequest, IdentityVerificationResponse>(
            "api/trust/me/identity/start",
            request,
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<TrustStatusResponse>> AcceptRulesAsync(
        AcceptTrustRulesRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<AcceptTrustRulesRequest, TrustStatusResponse>(
            "api/trust/me/rules/accept",
            request,
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<TrustStatusResponse>> SubmitGuideProfileReviewAsync(
        SubmitManualGuideReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<SubmitManualGuideReviewRequest, TrustStatusResponse>(
            "api/trust/me/guide-profile/submit-review",
            request,
            cancellationToken: cancellationToken);
    }
}
