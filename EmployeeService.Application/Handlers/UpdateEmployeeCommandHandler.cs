using EmployeeService.Application.Commands;
using EmployeeService.Application.Dtos;
using EmployeeService.Application.Exceptions;
using EmployeeService.Domain.Interfaces;
using MediatR;

namespace EmployeeService.Application.Handlers;

public class UpdateEmployeeCommandHandler(IEmployeeRepository repository)
    : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (employee == null)
            throw new NotFoundException($"Employee with ID {request.Id} not found");

        employee.Name = request.Name;
        employee.Email = request.Email;
        employee.Position = request.Position;

        await repository.UpdateAsync(employee, cancellationToken);

        return new EmployeeDto(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.Position,
            employee.UserId
        );
    }
}