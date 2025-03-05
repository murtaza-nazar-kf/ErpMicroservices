using EmployeeService.Application.Commands;
using EmployeeService.Application.Dtos;
using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Interfaces;
using MediatR;

namespace EmployeeService.Application.Handlers;

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

        var createdEmployee = await repository.AddAsync(employee, cancellationToken);

        return new EmployeeDto(
            createdEmployee.Id,
            createdEmployee.Name,
            createdEmployee.Email,
            createdEmployee.Position,
            createdEmployee.UserId
        );
    }
}