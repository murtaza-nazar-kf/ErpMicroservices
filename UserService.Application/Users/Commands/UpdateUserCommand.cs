using MediatR;

namespace UserService.Application.Users.Commands;

public record UpdateUserCommand(Guid Id, string Username, string Email) : IRequest<bool>;