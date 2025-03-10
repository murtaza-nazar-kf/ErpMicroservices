using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace UserService.Infrastructure.Identity;

public class KeycloakJwtMiddleware
{
    private readonly RequestDelegate _next;

    public KeycloakJwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var user = context.User;
        if (user.Identity.IsAuthenticated)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            var realmRoles = user.FindFirst("realm_access")?.Value;
            if (!string.IsNullOrEmpty(realmRoles))
            {
                var roles = JsonSerializer.Deserialize<Dictionary<string, string[]>>(realmRoles);
                if (roles != null && roles.TryGetValue("roles", out var userRoles))
                    foreach (var role in userRoles)
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        await _next(context);
    }
}