﻿using EmployeeService.Application.Exceptions;
using EmployeeService.Domain.Interfaces;
using MediatR;

namespace EmployeeService.Application.Employees.Commands.Handlers;

public class DeleteEmployeeCommandHandler(IEmployeeRepository repository)
    : IRequestHandler<DeleteEmployeeCommand, Unit>
{
    public async Task<Unit> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (employee == null)
            throw new NotFoundException($"Employee with ID {request.Id} not found");

        await repository.DeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}