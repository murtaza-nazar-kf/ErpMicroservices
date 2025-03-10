namespace EmployeeService.Application.Employees.Dtos;

public record CreateEmployeeDto(
    string Name,
    string Email,
    string Position,
    Guid UserId
);