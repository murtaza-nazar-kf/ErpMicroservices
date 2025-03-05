using EmployeeService.Application.Dtos;
using MediatR;

namespace EmployeeService.Application.Commands;

public record UpdateEmployeeCommand(
    Guid Id,
    string Name,
    string Email,
    string Position
) : IRequest<EmployeeDto>;