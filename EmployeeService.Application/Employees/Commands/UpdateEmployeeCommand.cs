using EmployeeService.Application.Employees.Dtos;
using MediatR;

namespace EmployeeService.Application.Employees.Commands;

public record UpdateEmployeeCommand(
    Guid Id,
    string Name,
    string Email,
    string Position
) : IRequest<EmployeeDto>;