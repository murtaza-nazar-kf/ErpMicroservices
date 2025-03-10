using System.Security.Claims;
using EmployeeService.Domain.Interfaces;

namespace EmployeeService.Infrastructure.Identity;

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