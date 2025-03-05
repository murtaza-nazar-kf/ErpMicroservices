using EmployeeService.Application.Dtos;
using MediatR;

namespace EmployeeService.Application.Commands;

public record CreateEmployeeCommand(
    string Name,
    string Email,
    string Position,
    Guid UserId
) : IRequest<EmployeeDto>;