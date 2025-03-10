using MediatR;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries;

public record GetUserQuery(Guid Id) : IRequest<UserDto>;