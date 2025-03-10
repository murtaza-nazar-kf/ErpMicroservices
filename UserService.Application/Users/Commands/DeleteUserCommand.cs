using MediatR;

namespace UserService.Application.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<bool>;