using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserService.Application.Commands;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Messaging;

namespace UserService.Application.Handlers;

public class CreateUserHandler(
    IUserRepository repository,
    ILogger<CreateUserHandler> logger,
    RabbitMqConnection rabbitMq)
    : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting user creation process for email: {Email}", request.Email);

        try
        {
            if (await repository.EmailExistsAsync(request.Email))
            {
                logger.LogWarning("Email already exists: {Email}", request.Email);
                throw new ApplicationException("Email already exists");
            }

            if (await repository.UsernameExistsAsync(request.Username))
            {
                logger.LogWarning("Username already exists: {Username}", request.Username);
                throw new ApplicationException("Username already exists");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await repository.CreateAsync(user);
            logger.LogInformation("Successfully created user with ID: {UserId}", user.Id);

            // Publish event to RabbitMQ
            var userEvent = JsonConvert.SerializeObject(new { user.Id, user.Username, user.Email });
            rabbitMq.Publish(userEvent);

            return user.Id;
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            logger.LogError(ex, "Failed to create user with email: {Email}", request.Email);
            throw;
        }
    }
}