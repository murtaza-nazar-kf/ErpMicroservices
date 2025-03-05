using MediatR;

namespace EmployeeService.Application.Commands;

public record DeleteEmployeeCommand(Guid Id) : IRequest<Unit>;