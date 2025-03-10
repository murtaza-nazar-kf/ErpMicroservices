using System.Text;
using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Events;
using EmployeeService.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmployeeService.Infrastructure.Consumers;

public class UserCreatedConsumer(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSettings> rabbitMqOptions,
    IConnection connection,
    ILogger<UserCreatedConsumer> logger)
    : BackgroundService
{
    private IModel? _channel;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CreateConnection();
        StartConsuming(stoppingToken);
        return Task.CompletedTask;
    }

    private void CreateConnection()
    {
        _channel = connection.CreateModel();
        _channel.QueueDeclare(
            rabbitMqOptions.Value.UserCreatedQueue,
            true,
            false,
            false
        );

        logger.LogInformation("RabbitMQ connection and queue created");
    }

    private void StartConsuming(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            if (stoppingToken.IsCancellationRequested) return;

            var userEvent = DeserializeMessage(ea.Body.ToArray());
            if (userEvent == null)
            {
                logger.LogWarning("Received a message that could not be deserialized.");
                return;
            }

            await HandleUserCreatedEventAsync(userEvent).ConfigureAwait(false);
        };

        _channel.BasicConsume(
            rabbitMqOptions.Value.UserCreatedQueue,
            true,
            consumer
        );

        logger.LogInformation("Started consuming messages from queue: {QueueName}",
            rabbitMqOptions.Value.UserCreatedQueue);
    }

    private UserCreatedEvent? DeserializeMessage(byte[] body)
    {
        try
        {
            var message = Encoding.UTF8.GetString(body);
            return JsonConvert.DeserializeObject<UserCreatedEvent>(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deserializing RabbitMQ message: {Message}", Encoding.UTF8.GetString(body));
            return null;
        }
    }

    private async Task HandleUserCreatedEventAsync(UserCreatedEvent userEvent)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();

        var employeeExists = await dbContext.Employees
            .AnyAsync(e => e.UserId == userEvent.Id).ConfigureAwait(false);

        if (!employeeExists)
        {
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                Name = userEvent.Username,
                Email = userEvent.Email,
                Position = "New Employee",
                UserId = userEvent.Id
            };

            await AddEmployeeAsync(dbContext, employee).ConfigureAwait(false);
        }
        else
        {
            logger.LogInformation("Employee with UserId {UserId} already exists in the database", userEvent.Id);
        }
    }

    private async Task AddEmployeeAsync(EmployeeDbContext dbContext, Employee employee)
    {
        try
        {
            dbContext.Employees.Add(employee);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            logger.LogInformation("Successfully added new employee with UserId {UserId}", employee.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving employee with UserId {UserId} to the database", employee.UserId);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        connection?.Close();
        GC.SuppressFinalize(this);
        logger.LogInformation("Disposed RabbitMQ connection and channel");
        base.Dispose();
    }
}