using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries;

public record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;