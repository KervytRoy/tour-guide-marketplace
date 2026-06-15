using TourGuideMarketplace.Admin.Infrastructure.Api;
using TourGuideMarketplace.Contracts.Admin;
using TourGuideMarketplace.Contracts.Common;

namespace TourGuideMarketplace.Admin.Features.Verifications;

public sealed class AdminVerificationsApiClient
{
    private readonly ApiClient _apiClient;

    public AdminVerificationsApiClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<PagedResult<AdminVerificationSummaryResponse>>> SearchAsync(
        AdminVerificationSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"pageNumber={request.PageNumber}",
            $"pageSize={request.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query.Add($"status={Uri.EscapeDataString(request.Status.Trim())}");
        }

        return _apiClient.GetAsync<PagedResult<AdminVerificationSummaryResponse>>(
            $"api/admin/verifications?{string.Join("&", query)}",
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<AdminVerificationDetailResponse>> GetDetailAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<AdminVerificationDetailResponse>(
            $"api/admin/verifications/{userId}",
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<AdminVerificationDetailResponse>> UpdateManualReviewAsync(
        Guid userId,
        AdminManualReviewUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<AdminManualReviewUpdateRequest, AdminVerificationDetailResponse>(
            $"api/admin/verifications/{userId}/manual-review",
            request,
            cancellationToken: cancellationToken);
    }

    public Task<ApiResult<AdminVerificationDetailResponse>> ApproveProfileAsync(
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostDecisionAsync(userId, "approve-profile", request, cancellationToken);
    }

    public Task<ApiResult<AdminVerificationDetailResponse>> RejectProfileAsync(
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostDecisionAsync(userId, "reject-profile", request, cancellationToken);
    }

    public Task<ApiResult<AdminVerificationDetailResponse>> SuspendAsync(
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostDecisionAsync(userId, "suspend", request, cancellationToken);
    }

    public Task<ApiResult<AdminVerificationDetailResponse>> ReactivateAsync(
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostDecisionAsync(userId, "reactivate", request, cancellationToken);
    }

    private Task<ApiResult<AdminVerificationDetailResponse>> PostDecisionAsync(
        Guid userId,
        string action,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        return _apiClient.PostAsync<AdminReviewDecisionRequest, AdminVerificationDetailResponse>(
            $"api/admin/verifications/{userId}/{action}",
            request,
            cancellationToken: cancellationToken);
    }
}
