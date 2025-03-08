using System.Security.Claims;
using Keycloak.AuthServices.Authorization;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserService.Application.Extensions;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Logging;
using UserService.Infrastructure.Messaging;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;

namespace UserService.Api.Configurations;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration,
        IHostBuilder hostBuilder)
    {
        // 🔹 Configure CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", policy =>
            {
                policy.WithOrigins("http://m.erp.com", "http://users.m.erp.com", "http://auth.m.erp.com",
                        "http://employee.m.erp.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Authorization");
            });
        });

        // 🔹 Authentication & Authorization
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "http://auth.m.erp.com/realms/microservices-realm";
                options.Audience = "dotnet-client";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "http://auth.m.erp.com/realms/microservices-realm",
                    ValidateAudience = true,
                    ValidAudiences = ["dotnet-client", "realm-management", "broker", "account"],
                    ValidateLifetime = true,
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        if (claimsIdentity != null)
                        {
                            var realmRoles = context.Principal?.FindFirst("realm_access")?.Value;
                            if (!string.IsNullOrEmpty(realmRoles))
                            {
                                var roles = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string[]>>(realmRoles);
                                if (roles != null && roles.TryGetValue("roles", out var userRoles))
                                {
                                    foreach (var role in userRoles)
                                    {
                                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                                    }
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services
            .AddAuthorization()
            .AddKeycloakAuthorization();

        const string policyName = "RequireAdminRole";
        const string adminRole = "hr-admin";

        services.AddAuthorizationBuilder()
            .AddPolicy(policyName, policy => policy.RequireRealmRoles(adminRole));

        // 🔹 Logging Configuration (Serilog)
        hostBuilder.AddSerilogConfiguration();

        // 🔹 Register Application Services

        // Add RabbitMQ connection as a singleton with configuration
        services.AddSingleton<RabbitMqConnection>(sp =>
            new RabbitMqConnection(
                sp.GetRequiredService<ILogger<RabbitMqConnection>>(),
                sp.GetRequiredService<IConfiguration>()
            )
        );


        services.AddApplicationServices();
        services.AddGrpc(options => options.EnableDetailedErrors = true);
        services.AddGrpcReflection();
        services.AddScoped<IUserRepository, UserRepository>();

        // 🔹 Database Configuration
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // 🔹 Controllers & Swagger Configuration
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        ConfigureSwagger(services);

        services.AddFluentValidationRulesToSwagger();
        services.AddHttpContextAccessor();
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Secured API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}