using MediatR;

namespace EmployeeService.Application.Employees.Commands;

public record DeleteEmployeeCommand(Guid Id) : IRequest<Unit>;