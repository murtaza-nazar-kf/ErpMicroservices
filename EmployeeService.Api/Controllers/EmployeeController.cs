using EmployeeService.Application.Employees.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.API.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeeController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command)
    {
        var employeeId = await mediator.Send(command);
        return CreatedAtAction(nameof(CreateEmployee), new { id = employeeId }, employeeId);
    }
}