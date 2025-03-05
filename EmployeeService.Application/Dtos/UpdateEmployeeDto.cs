namespace EmployeeService.Application.Dtos;

public record UpdateEmployeeDto(
    Guid Id,
    string Name,
    string Email,
    string Position
);