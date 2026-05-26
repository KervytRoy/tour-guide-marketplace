using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TourGuideMarketplace.Application.Common.Models;
using TourGuideMarketplace.Application.Common.Users;
using TourGuideMarketplace.Infrastructure.Persistence;

namespace TourGuideMarketplace.Infrastructure.Identity;

internal sealed class IdentityUserAccountService : IUserAccountService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserAccountService(
        ApplicationDbContext dbContext,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : Map(user);
    }

    public async Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : Map(user);
    }

    public async Task<IReadOnlyCollection<UserAccount>> ListAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Select(user => Map(user))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserAccount>> ListByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .Select(user => Map(user))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? [] : (await _userManager.GetRolesAsync(user)).ToArray();
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> IsLockedOutAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && await _userManager.IsLockedOutAsync(user);
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task AccessFailedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            await _userManager.AccessFailedAsync(user);
        }
    }

    public async Task ResetAccessFailedCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }
    }

    public async Task<Result<UserAccount>> CreateAsync(
        CreateUserAccountRequest request,
        CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        return result.Succeeded
            ? Result<UserAccount>.Success(Map(user))
            : Result<UserAccount>.Failure(result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<Result> EnsureRoleExistsAsync(string role, CancellationToken cancellationToken)
    {
        if (await _roleManager.RoleExistsAsync(role))
        {
            return Result.Success();
        }

        var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<Result> AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(error => error.Description).ToArray());
    }

    public Task<Result> SetEmailConfirmedAsync(Guid userId, bool confirmed, CancellationToken cancellationToken)
    {
        return UpdateUserAsync(userId, user => user.EmailConfirmed = confirmed);
    }

    public Task<Result> SetPhoneNumberConfirmedAsync(Guid userId, bool confirmed, CancellationToken cancellationToken)
    {
        return UpdateUserAsync(userId, user => user.PhoneNumberConfirmed = confirmed);
    }

    public Task<Result> SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken)
    {
        return UpdateUserAsync(userId, user => user.IsActive = isActive);
    }

    private async Task<Result> UpdateUserAsync(Guid userId, Action<ApplicationUser> update)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        update(user);
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(error => error.Description).ToArray());
    }

    private static UserAccount Map(ApplicationUser user)
    {
        return new UserAccount(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            user.PhoneNumber,
            user.EmailConfirmed,
            user.PhoneNumberConfirmed,
            user.IsActive);
    }
}
