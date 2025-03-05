using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Exceptions;
using UserService.Application.Queries;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers;

public class GetUserHandler(IUserRepository repository, ILogger<GetUserHandler> logger)
    : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving user with ID: {UserId}", request.Id);

        try
        {
            var user = await repository.GetByIdAsync(request.Id);
            if (user == null)
            {
                logger.LogWarning("User not found with ID: {UserId}", request.Id);
                throw new NotFoundException($"User with ID {request.Id} not found");
            }

            var dto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };

            logger.LogInformation("Successfully retrieved user with ID: {UserId}", request.Id);
            return dto;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            logger.LogError(ex, "Error retrieving user with ID: {UserId}", request.Id);
            throw;
        }
    }
}