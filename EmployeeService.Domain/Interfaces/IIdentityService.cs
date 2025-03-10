using System.Security.Claims;

namespace EmployeeService.Domain.Interfaces;

public interface IIdentityService
{
    Task<bool> IsUserInRoleAsync(ClaimsPrincipal user, string role);
    string GetUserId(ClaimsPrincipal user);
}