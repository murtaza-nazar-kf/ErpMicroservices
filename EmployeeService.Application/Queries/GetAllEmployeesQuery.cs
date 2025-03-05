using EmployeeService.Application.Dtos;
using MediatR;

namespace EmployeeService.Application.Queries;

public record GetAllEmployeesQuery : IRequest<IEnumerable<EmployeeDto>>;