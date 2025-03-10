using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace EmployeeService.Infrastructure.Identity;

public class KeycloakJwtMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user.Identity is { IsAuthenticated: true })
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            var realmRoles = user.FindFirst("realm_access")?.Value;
            if (string.IsNullOrEmpty(realmRoles)) return next(context);
            var roles = JsonSerializer.Deserialize<Dictionary<string, string[]>>(realmRoles);
            if (roles != null && roles.TryGetValue("roles", out var userRoles))
                foreach (var role in userRoles)
                    claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return next(context);
    }
}