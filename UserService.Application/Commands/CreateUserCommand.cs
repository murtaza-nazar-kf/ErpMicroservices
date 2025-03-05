using MediatR;

namespace UserService.Application.Commands;

public record CreateUserCommand(string Username, string Email, string Password) : IRequest<Guid>;