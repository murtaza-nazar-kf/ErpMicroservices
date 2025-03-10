namespace UserService.Domain.Interfaces;

public interface IMessageBroker
{
    void PublishMessage(string message);

    void PublishMessage(string message, string? queueName);
}