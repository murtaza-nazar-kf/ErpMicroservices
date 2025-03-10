using EmployeeService.Application.Employees.Dtos;
using EmployeeService.Domain.Interfaces;
using MediatR;

namespace EmployeeService.Application.Employees.Queries.Handlers;

public class GetAllEmployeesQueryHandler(IEmployeeRepository repository)
    : IRequestHandler<GetAllEmployeesQuery, IEnumerable<EmployeeDto>>
{
    public async Task<IEnumerable<EmployeeDto>> Handle(GetAllEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return employees.Select(employee => new EmployeeDto(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.Position,
            employee.UserId
        ));
    }
}