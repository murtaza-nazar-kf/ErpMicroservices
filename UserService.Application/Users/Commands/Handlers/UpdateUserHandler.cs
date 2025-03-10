using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Domain.Interfaces;

namespace UserService.Application.Users.Commands.Handlers;

public class UpdateUserHandler(IUserRepository repository, ILogger<UpdateUserHandler> logger)
    : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting update process for user ID: {UserId}", request.Id);

        try
        {
            var user = await repository.GetByIdAsync(request.Id);
            if (user == null)
            {
                logger.LogWarning("User not found with ID: {UserId}", request.Id);
                return false;
            }

            if (user.Email != request.Email && await repository.EmailExistsAsync(request.Email))
            {
                logger.LogWarning("Cannot update user {UserId}. Email already exists: {Email}", request.Id,
                    request.Email);
                throw new ApplicationException("Email already exists");
            }

            if (user.Username != request.Username && await repository.UsernameExistsAsync(request.Username))
            {
                logger.LogWarning("Cannot update user {UserId}. Username already exists: {Username}", request.Id,
                    request.Username);
                throw new ApplicationException("Username already exists");
            }

            user.Username = request.Username;
            user.Email = request.Email;

            await repository.UpdateAsync(user);
            logger.LogInformation("Successfully updated user with ID: {UserId}", user.Id);

            return true;
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            logger.LogError(ex, "Failed to update user with ID: {UserId}", request.Id);
            throw;
        }
    }
}