using System.Text;
using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Events;
using EmployeeService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmployeeService.Infrastructure.Consumers;

public class UserCreatedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<RabbitMqSettings> _rabbitMqOptions;
    private readonly IConnection _connection;
    private IModel _channel;

    public UserCreatedConsumer(
        IServiceProvider serviceProvider,
        IOptions<RabbitMqSettings> rabbitMqOptions,
        IConnection connection)
    {
        _serviceProvider = serviceProvider;
        _rabbitMqOptions = rabbitMqOptions;
        _connection = connection;
    }

    private void CreateConnection()
    {
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _rabbitMqOptions.Value.UserCreatedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CreateConnection(); // Create the channel

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var userEvent = JsonConvert.DeserializeObject<UserCreatedEvent>(message);

            if (userEvent == null) return;

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();

            // Check if employee already exists
            if (!await dbContext.Employees.AnyAsync(e => e.UserId == userEvent.Id))
            {
                var employee = new Employee
                {
                    Id = Guid.NewGuid(),
                    Name = userEvent.Username,
                    Email = userEvent.Email,
                    Position = "New Employee",
                    UserId = userEvent.Id
                };

                dbContext.Employees.Add(employee);
                await dbContext.SaveChangesAsync();
            }
        };

        _channel.BasicConsume(
            queue: _rabbitMqOptions.Value.UserCreatedQueue,
            autoAck: true,
            consumer: consumer
        );

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        GC.SuppressFinalize(this);
        base.Dispose();
    }
}
