using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers;

public class GetAllUsersHandler(IUserRepository repository, ILogger<GetAllUsersHandler> logger)
    : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all users");

        try
        {
            var users = await repository.GetAllAsync();
            var dtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            });

            logger.LogInformation("Successfully retrieved {Count} users", dtos.Count());
            return dtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }
}