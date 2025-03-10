using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Identity;

public static class AuthenticationExtensions
{
    private static void ConfigureJwtBearerOptions(JwtBearerOptions options, IConfiguration configuration)
    {
        var keycloakConfig = configuration.GetSection("Keycloak");
        if (keycloakConfig == null) throw new InvalidOperationException("Keycloak configuration section is missing.");

        options.Authority = keycloakConfig["Authority"];
        options.Audience = keycloakConfig["Audience"];
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Authority,
            ValidateAudience = true,
            ValidAudiences = keycloakConfig.GetSection("ValidAudience").Get<string[]>(),
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                AddRolesToClaimsIdentity(context.Principal?.Identity as ClaimsIdentity, context.Principal);
                return Task.CompletedTask;
            }
        };
    }

    private static void AddRolesToClaimsIdentity(ClaimsIdentity? claimsIdentity, ClaimsPrincipal? principal)
    {
        if (claimsIdentity == null || principal == null) return;

        var realmRoles = principal.FindFirst("realm_access")?.Value;
        if (string.IsNullOrEmpty(realmRoles)) return;

        var roles = JsonSerializer.Deserialize<Dictionary<string, string[]>>(realmRoles);
        if (roles == null || !roles.TryGetValue("roles", out var userRoles)) return;
        foreach (var role in userRoles) claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => { ConfigureJwtBearerOptions(options, configuration); });

        return services;
    }
}