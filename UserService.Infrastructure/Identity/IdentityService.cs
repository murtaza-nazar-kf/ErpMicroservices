using System.Security.Claims;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    public Task<bool> IsUserInRoleAsync(ClaimsPrincipal user, string role)
    {
        return Task.FromResult(user.IsInRole(role));
    }

    public string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }
}