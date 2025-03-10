using System.Security.Claims;
using EmployeeService.Application.Employees.Commands;
using EmployeeService.Application.Employees.Queries;
using EmployeeService.Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController(IMediator mediator, ILogger<EmployeeController> logger) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "hr-admin")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command)
    {
        try
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUsername = User.FindFirstValue(ClaimTypes.Name);
            logger.LogInformation("User {Username} (ID: {UserId}) is creating a new employee", currentUsername,
                currentUserId);

            var employeeId = await mediator.Send(command).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetEmployeeById), new { id = employeeId }, employeeId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating employee");
            return StatusCode(500, "An error occurred while creating the employee.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEmployeeById(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized("User is not authenticated.");

        if (id.ToString() != currentUserId && !User.IsInRole("hr-admin"))
            return Forbid();

        try
        {
            var employee = await mediator.Send(new GetEmployeeByIdQuery(id)).ConfigureAwait(false);
            return employee != null ? Ok(employee) : NotFound("Employee not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching employee {EmployeeId}", id);
            return StatusCode(500, "An error occurred while retrieving the employee.");
        }
    }

    [HttpGet]
    [Authorize(Roles = "hr-admin")]
    public async Task<IActionResult> GetAllEmployees()
    {
        try
        {
            var employees = await mediator.Send(new GetAllEmployeesQuery()).ConfigureAwait(false);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all employees");
            return StatusCode(500, "An error occurred while retrieving employees.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeCommand command)
    {
        if (id != command.Id)
            return BadRequest("Mismatched employee ID");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized("User is not authenticated.");

        if (id.ToString() != currentUserId && !User.IsInRole("hr-admin"))
            return Forbid();

        try
        {
            await mediator.Send(command).ConfigureAwait(false);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating employee {EmployeeId}", id);
            return StatusCode(500, "An error occurred while updating the employee.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized("User is not authenticated.");

        if (id.ToString() != currentUserId && !User.IsInRole("hr-admin"))
            return Forbid();

        try
        {
            await mediator.Send(new DeleteEmployeeCommand(id)).ConfigureAwait(false);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
            return StatusCode(500, "An error occurred while deleting the employee.");
        }
    }

    [HttpGet("me")]
    [AllowAnonymous]
    public IActionResult GetCurrentEmployeeInfo()
    {
        if (User.Identity is not { IsAuthenticated: true })
            return Ok(new { Message = "User not authenticated" });

        return Ok(new
        {
            EmployeeId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Username = User.FindFirstValue(ClaimTypes.Name),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role),
            IsAdmin = User.IsInRole("hr-admin")
        });
    }
}