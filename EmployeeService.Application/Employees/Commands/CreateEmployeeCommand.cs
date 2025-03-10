using EmployeeService.Application.Employees.Dtos;
using MediatR;

namespace EmployeeService.Application.Employees.Commands;

public record CreateEmployeeCommand(
    string Name,
    string Email,
    string Position,
    Guid UserId
) : IRequest<EmployeeDto>;