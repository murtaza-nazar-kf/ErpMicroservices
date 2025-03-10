using EmployeeService.Application.Employees.Dtos;
using EmployeeService.Application.Exceptions;
using EmployeeService.Domain.Interfaces;
using MediatR;

namespace EmployeeService.Application.Employees.Commands.Handlers;

public class UpdateEmployeeCommandHandler(IEmployeeRepository repository)
    : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (employee == null)
            throw new NotFoundException($"Employee with ID {request.Id} not found");

        employee.Name = request.Name;
        employee.Email = request.Email;
        employee.Position = request.Position;

        await repository.UpdateAsync(employee, cancellationToken).ConfigureAwait(false);

        return new EmployeeDto(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.Position,
            employee.UserId
        );
    }
}