using System.Text.Json;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Application.Trust;

namespace TourGuideMarketplace.Infrastructure.Trust;

internal sealed class MockIdentityVerificationProvider : IIdentityVerificationProvider
{
    private const string ProviderName = "MockMetaMap";

    public Task<IdentityVerificationProviderResult> StartAsync(
        IdentityVerificationProviderRequest request,
        CancellationToken cancellationToken)
    {
        var status = ResolveStatus(request);
        var externalId = $"mock_metamap_{Guid.NewGuid():N}";
        var last4 = GetLast4(request.DocumentNumber);
        var failureReason = status switch
        {
            IdentityVerificationProviderStatus.Rejected => "Mock provider rejected the identity document.",
            IdentityVerificationProviderStatus.InReview => "Mock provider requires manual review.",
            _ => null
        };

        var requestPayload = JsonSerializer.Serialize(new
        {
            flowAlias = "tour-guide-marketplace-latam-identity",
            country = request.Country,
            document = new
            {
                type = request.DocumentType,
                numberLast4 = last4
            },
            user = new
            {
                id = request.UserId,
                fullName = request.FullName,
                dateOfBirth = request.DateOfBirth
            }
        });

        var responsePayload = JsonSerializer.Serialize(new
        {
            id = externalId,
            provider = ProviderName,
            status = status.ToString(),
            failureReason,
            checks = new
            {
                document = status == IdentityVerificationProviderStatus.Approved ? "passed" : "needs_review",
                selfieLiveness = status == IdentityVerificationProviderStatus.Approved ? "passed" : "not_completed"
            }
        });

        return Task.FromResult(new IdentityVerificationProviderResult(
            ProviderName,
            externalId,
            status,
            failureReason,
            requestPayload,
            responsePayload));
    }

    private static IdentityVerificationProviderStatus ResolveStatus(IdentityVerificationProviderRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.RequestedMockOutcome)
            && Enum.TryParse<IdentityVerificationProviderStatus>(
                request.RequestedMockOutcome.Trim(),
                ignoreCase: true,
                out var requestedStatus))
        {
            return requestedStatus;
        }

        var documentNumber = request.DocumentNumber.Trim();
        if (documentNumber.EndsWith("0000", StringComparison.Ordinal))
        {
            return IdentityVerificationProviderStatus.Rejected;
        }

        if (documentNumber.EndsWith("9999", StringComparison.Ordinal))
        {
            return IdentityVerificationProviderStatus.InReview;
        }

        return IdentityVerificationProviderStatus.Approved;
    }

    private static string? GetLast4(string value)
    {
        var normalized = value.Trim();
        return normalized.Length <= 4 ? normalized : normalized[^4..];
    }
}
