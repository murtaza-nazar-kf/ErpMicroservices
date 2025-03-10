using System.Security.Claims;

namespace UserService.Domain.Interfaces;

public interface IIdentityService
{
    Task<bool> IsUserInRoleAsync(ClaimsPrincipal user, string role);
    string GetUserId(ClaimsPrincipal user);
}