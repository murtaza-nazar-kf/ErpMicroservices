namespace EmployeeService.Application.Dtos;

public record CreateEmployeeDto(
    string Name,
    string Email,
    string Position,
    Guid UserId
);