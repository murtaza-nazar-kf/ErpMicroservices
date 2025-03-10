namespace EmployeeService.Application.Employees.Dtos;

public record UpdateEmployeeDto(
    Guid Id,
    string Name,
    string Email,
    string Position
);