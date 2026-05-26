using TourGuideMarketplace.Application.Trust;

namespace TourGuideMarketplace.Application.Interfaces;

public interface IIdentityVerificationProvider
{
    Task<IdentityVerificationProviderResult> StartAsync(
        IdentityVerificationProviderRequest request,
        CancellationToken cancellationToken);
}
