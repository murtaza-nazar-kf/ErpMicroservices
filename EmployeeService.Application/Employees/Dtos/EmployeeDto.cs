namespace EmployeeService.Application.Employees.Dtos;

public record EmployeeDto(
    Guid Id,
    string Name,
    string Email,
    string Position,
    Guid UserId
);