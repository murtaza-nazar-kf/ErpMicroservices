namespace EmployeeService.Infrastructure.Consumers;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "rabbitmq";
    public string UserName { get; set; } = "user";
    public string Password { get; set; } = "password";
    public int Port { get; set; } = 5672;
    public int ConnectionTimeout { get; set; } = 10;
    public string UserCreatedQueue { get; set; } = "user-created";
}