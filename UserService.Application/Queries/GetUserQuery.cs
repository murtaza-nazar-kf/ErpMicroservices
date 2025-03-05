using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries;

public record GetUserQuery(Guid Id) : IRequest<UserDto>;