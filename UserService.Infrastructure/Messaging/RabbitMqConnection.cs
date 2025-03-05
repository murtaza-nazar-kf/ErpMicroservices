using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace UserService.Infrastructure.Messaging;

public class RabbitMqConnection : IDisposable
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly string _queueName;

    public RabbitMqConnection(
        ILogger<RabbitMqConnection> logger,
        IConfiguration configuration)
    {
        _logger = logger;

        // Correct configuration section retrieval
        var rabbitMqConfig = configuration.GetSection("RabbitMQ");

        var hostName = rabbitMqConfig["HostName"] ?? "rabbitmq";
        var userName = rabbitMqConfig["UserName"] ?? "user";
        var password = rabbitMqConfig["Password"] ?? "password";
        _queueName = rabbitMqConfig["QueueName"] ?? "user-created";

        _logger.LogInformation("Establishing connection to RabbitMQ...");

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare the queue with the correct name
        _channel.QueueDeclare(_queueName, true, false, false);

        _logger.LogInformation("Connection established and queue declared");
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        GC.SuppressFinalize(this);
        _logger.LogInformation("RabbitMQ connection and channel closed");
    }

    public void Publish(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish("", _queueName, null, body);
        _logger.LogInformation("Message published to '{QueueName}' queue: {Message}", _queueName, message);
    }
}