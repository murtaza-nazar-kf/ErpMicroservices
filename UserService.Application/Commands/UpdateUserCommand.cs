using MediatR;

namespace UserService.Application.Commands;

public record UpdateUserCommand(Guid Id, string Username, string Email) : IRequest<bool>;