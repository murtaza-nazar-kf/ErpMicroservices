using EmployeeService.Application.Employees.Dtos;
using MediatR;

namespace EmployeeService.Application.Employees.Queries;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto>;