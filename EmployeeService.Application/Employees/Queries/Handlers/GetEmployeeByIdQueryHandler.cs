using EmployeeService.Application.Employees.Dtos;
using EmployeeService.Application.Exceptions;
using EmployeeService.Domain.Interfaces;
using MediatR;

namespace EmployeeService.Application.Employees.Queries.Handlers;

public class GetEmployeeByIdQueryHandler(IEmployeeRepository repository)
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        return employee == null
            ? throw new NotFoundException($"Employee with ID {request.Id} not found")
            : new EmployeeDto(
                employee.Id,
                employee.Name,
                employee.Email,
                employee.Position,
                employee.UserId
            );
    }
}