using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Messaging;

public class RabbitMqMessageBroker : IMessageBroker, IDisposable
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly string? _defaultQueueName;
    private readonly ILogger<RabbitMqMessageBroker> _logger;
    private bool _disposed;

    public RabbitMqMessageBroker(
        ILogger<RabbitMqMessageBroker> logger,
        IConfiguration configuration)
    {
        _logger = logger;

        // Retrieve configuration
        var rabbitMqConfig = configuration.GetSection("RabbitMQ");

        var hostName = rabbitMqConfig["HostName"];
        var userName = rabbitMqConfig["UserName"];
        var password = rabbitMqConfig["Password"];
        _defaultQueueName = rabbitMqConfig["QueueName"];

        _logger.LogInformation("Establishing connection to RabbitMQ...");

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(_defaultQueueName, true, false, false);

        _logger.LogInformation("Connection established and queue declared");
    }

    // Implement Dispose method
    public void Dispose()
    {
        if (_disposed) return;
        _channel?.Close();
        _connection?.Close();
        _disposed = true;
        GC.SuppressFinalize(this);
        _logger.LogInformation("RabbitMQ connection and channel closed");
    }

    public void PublishMessage(string message)
    {
        PublishToQueue(message, _defaultQueueName);
    }

    public void PublishMessage(string message, string? queueName)
    {
        PublishToQueue(message, queueName);
    }

    private void PublishToQueue(string message, string? queueName)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish("", queueName, null, body);
        _logger.LogInformation("Message published to '{QueueName}' queue: {Message}", queueName, message);
    }
}