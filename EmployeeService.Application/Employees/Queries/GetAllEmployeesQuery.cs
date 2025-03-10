using EmployeeService.Application.Employees.Dtos;
using MediatR;

namespace EmployeeService.Application.Employees.Queries;

public record GetAllEmployeesQuery : IRequest<IEnumerable<EmployeeDto>>;