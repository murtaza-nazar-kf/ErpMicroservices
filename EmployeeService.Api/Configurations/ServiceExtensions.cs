using EmployeeService.Application.Employees.Extensions;
using EmployeeService.Domain.Interfaces;
using EmployeeService.Infrastructure.Consumers;
using EmployeeService.Infrastructure.Grpc.Services;
using EmployeeService.Infrastructure.Identity;
using EmployeeService.Infrastructure.Logging;
using EmployeeService.Infrastructure.Persistence.DbContexts;
using EmployeeService.Infrastructure.Persistence.Repositories;
using EmployeeService.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EmployeeService.Api.Configurations;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration,
        IHostBuilder hostBuilder)
    {
        ConfigureConfiguration(services, configuration);
        hostBuilder.AddSerilogConfiguration();
        ConfigureApiServices(services);
        ConfigureDatabaseServices(services, configuration);
        ConfigureApplicationServices(services);
        ConfigureExternalServices(services);

        // 🔹 Authentication & Authorization
        services.AddCustomAuthentication(configuration);
        services.AddCustomAuthorization();

        services.AddScoped<MigrationService>();
    }

    private static void ConfigureConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));
        services.AddSingleton<IConnection>(sp =>
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
                Console.WriteLine($"RabbitMQ Connection Error: {ex.Message}");
                throw;
            }
        });
    }

    private static void ConfigureApiServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    private static void ConfigureDatabaseServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EmployeeDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }

    private static void ConfigureApplicationServices(IServiceCollection services)
    {
        services.AddApplicationServices();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    }

    private static void ConfigureExternalServices(IServiceCollection services)
    {
        services.AddSingleton<UserGrpcClient>();
        services.AddTransient(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            return connection.CreateModel();
        });

        services.AddHostedService<UserCreatedConsumer>();
    }
}