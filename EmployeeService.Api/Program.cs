using EmployeeService.Application.Extensions;
using EmployeeService.Domain.Interfaces;
using EmployeeService.Infrastructure.Consumers;
using EmployeeService.Infrastructure.Grpc.Services;
using EmployeeService.Infrastructure.Logging;
using EmployeeService.Infrastructure.Persistence;
using EmployeeService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EmployeeService.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure services
        ConfigureServices(builder);

        // Build and run the application
        var app = builder.Build();
        ConfigureApplication(app);
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Configuration
        ConfigureConfiguration(builder);

        // Logging configuration
        builder.Host.AddSerilogConfiguration();

        // API and Swagger configuration
        ConfigureApiServices(builder);

        // Database configuration
        ConfigureDatabaseServices(builder);

        // Application-specific services
        ConfigureApplicationServices(builder);

        // External service integrations
        ConfigureExternalServices(builder);
    }

    private static void ConfigureConfiguration(WebApplicationBuilder builder)
    {
        // Add configuration sources
        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
            .AddEnvironmentVariables();

        // Bind RabbitMQ settings
        builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

        // Configure RabbitMQ connection
        builder.Services.AddSingleton<IConnection>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
            var factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = settings.UserName,
                Password = settings.Password,
                Port = settings.Port,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
            };

            try
            {
                return factory.CreateConnection();
            }
            catch (Exception ex)
            {
                // Log the specific connection error
                Console.WriteLine($"RabbitMQ Connection Error: {ex.Message}");
                throw;
            }
        });
    }

    private static void ConfigureApiServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    private static void ConfigureDatabaseServices(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<EmployeeDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    }

    private static void ConfigureApplicationServices(WebApplicationBuilder builder)
    {
        // Add application-specific services from extensions
        builder.Services.AddApplicationServices();
        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    }

    private static void ConfigureExternalServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<UserGrpcClient>();

        // Register IModel as a transient dependency to be created per operation
        builder.Services.AddTransient(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            return connection.CreateModel();
        });

        // Register the hosted service that will consume RabbitMQ messages
        builder.Services.AddHostedService<UserCreatedConsumer>();
    }

    private static void ConfigureApplication(WebApplication app)
    {
        // Development-specific configuration
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Middleware configuration
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        // Run the application
        app.Run();
    }
}
