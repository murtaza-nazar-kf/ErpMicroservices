using MediatR;

namespace UserService.Application.Users.Commands;

public record CreateUserCommand(string Username, string Email, string Password) : IRequest<Guid>;