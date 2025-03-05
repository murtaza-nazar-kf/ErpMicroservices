using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeService.Application.Commands;

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