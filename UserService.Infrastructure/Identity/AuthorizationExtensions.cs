using Keycloak.AuthServices.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace UserService.Infrastructure.Identity;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        const string policyName = "RequireAdminRole";
        const string adminRole = "hr-admin";

        services.AddAuthorization()
            .AddKeycloakAuthorization();

        services.AddAuthorizationBuilder()
            .AddPolicy(policyName, policy => policy.RequireRealmRoles(adminRole));

        return services;
    }
}