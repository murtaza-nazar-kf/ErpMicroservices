using EmployeeService.Application.Employees.Dtos;
using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Interfaces;
using MediatR;

namespace EmployeeService.Application.Employees.Commands.Handlers;

public class CreateEmployeeCommandHandler(IEmployeeRepository repository)
    : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Position = request.Position,
            UserId = request.UserId
        };

        var createdEmployee = await repository.AddAsync(employee, cancellationToken).ConfigureAwait(false);

        return new EmployeeDto(
            createdEmployee.Id,
            createdEmployee.Name,
            createdEmployee.Email,
            createdEmployee.Position,
            createdEmployee.UserId
        );
    }
}