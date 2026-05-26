using TourGuideMarketplace.Application.Common.Models;

namespace TourGuideMarketplace.Application.Common.Users;

public interface IUserAccountService
{
    Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserAccount>> ListAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserAccount>> ListByIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken);
    Task<bool> IsLockedOutAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken);
    Task AccessFailedAsync(Guid userId, CancellationToken cancellationToken);
    Task ResetAccessFailedCountAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<UserAccount>> CreateAsync(CreateUserAccountRequest request, CancellationToken cancellationToken);
    Task<Result> EnsureRoleExistsAsync(string role, CancellationToken cancellationToken);
    Task<Result> AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken);
    Task<Result> SetEmailConfirmedAsync(Guid userId, bool confirmed, CancellationToken cancellationToken);
    Task<Result> SetPhoneNumberConfirmedAsync(Guid userId, bool confirmed, CancellationToken cancellationToken);
    Task<Result> SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken);
}
