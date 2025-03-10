using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserService.Application.Users.Extensions;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Identity;
using UserService.Infrastructure.Logging;
using UserService.Infrastructure.Messaging;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Persistence.Repositories;

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
        services.AddCustomAuthentication(configuration);
        services.AddCustomAuthorization();

        // 🔹 Logging Configuration (Serilog)
        hostBuilder.AddSerilogConfiguration();

        // 🔹 Register Application Services

        // Add RabbitMQ connection as a singleton with configuration
        services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>(sp =>
            new RabbitMqMessageBroker(
                sp.GetRequiredService<ILogger<RabbitMqMessageBroker>>(),
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