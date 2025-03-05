using EmployeeService.Application.Dtos;
using MediatR;

namespace EmployeeService.Application.Queries;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto>;