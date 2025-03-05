using EmployeeService.Application.Dtos;
using EmployeeService.Application.Exceptions;
using EmployeeService.Application.Queries;
using EmployeeService.Domain.Interfaces;
using MediatR;

namespace EmployeeService.Application.Handlers;

public class GetEmployeeByIdQueryHandler(IEmployeeRepository repository)
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken);

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