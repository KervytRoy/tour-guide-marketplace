using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Contracts.Admin;
using TourGuideMarketplace.Contracts.Common;

namespace TourGuideMarketplace.Application.Interfaces;

public interface IAdminTrustService
{
    Task<Result<PagedResult<AdminVerificationSummaryResponse>>> SearchAsync(
        AdminVerificationSearchRequest request,
        CancellationToken cancellationToken);

    Task<Result<AdminVerificationDetailResponse>> GetDetailAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Result<AdminVerificationDetailResponse>> UpdateManualReviewAsync(
        Guid adminUserId,
        Guid userId,
        AdminManualReviewUpdateRequest request,
        CancellationToken cancellationToken);

    Task<Result<AdminVerificationDetailResponse>> ApproveProfileAsync(
        Guid adminUserId,
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken);

    Task<Result<AdminVerificationDetailResponse>> RejectProfileAsync(
        Guid adminUserId,
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken);

    Task<Result<AdminVerificationDetailResponse>> SuspendAsync(
        Guid adminUserId,
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken);

    Task<Result<AdminVerificationDetailResponse>> ReactivateAsync(
        Guid adminUserId,
        Guid userId,
        AdminReviewDecisionRequest request,
        CancellationToken cancellationToken);
}
