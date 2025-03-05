using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers;

public class DeleteUserHandler(IUserRepository repository, ILogger<DeleteUserHandler> logger)
    : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting deletion process for user ID: {UserId}", request.Id);

        try
        {
            if (!await repository.ExistsAsync(request.Id))
            {
                logger.LogWarning("User not found for deletion. ID: {UserId}", request.Id);
                return false;
            }

            await repository.DeleteAsync(request.Id);
            logger.LogInformation("Successfully deleted user with ID: {UserId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete user with ID: {UserId}", request.Id);
            throw;
        }
    }
}