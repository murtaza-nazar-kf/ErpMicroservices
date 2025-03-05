using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Commands;
using UserService.Application.Exceptions;
using UserService.Application.Queries;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController(IMediator mediator, ILogger<UserController> logger) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "hr-admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        try
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUsername = User.FindFirstValue(ClaimTypes.Name);
            logger.LogInformation("User {Username} (ID: {UserId}) is creating a new user", currentUsername,
                currentUserId);
            var userId = await mediator.Send(command);
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id.ToString() != currentUserId && !User.IsInRole("hr-admin")) return Forbid();
        var user = await mediator.Send(new GetUserQuery(id));
        return user is not null ? Ok(user) : NotFound();
    }

    [HttpGet]
    [Authorize(Roles = "hr-admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await mediator.Send(new GetAllUsersQuery());
        return Ok(users);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.Id) return BadRequest("Mismatched user ID");
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id.ToString() != currentUserId && !User.IsInRole("hr-admin")) return Forbid();
        try
        {
            await mediator.Send(command);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id.ToString() != currentUserId && !User.IsInRole("hr-admin")) return Forbid();
        try
        {
            await mediator.Send(new DeleteUserCommand(id));
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user.");
        }
    }

    [HttpGet("me")]
    [AllowAnonymous]
    public IActionResult GetCurrentUserInfo()
    {
        if (!User.Identity.IsAuthenticated) return Ok(new { Message = "User not authenticated" });

        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Username = User.FindFirstValue(ClaimTypes.Name),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role),
            IsAdmin = User.IsInRole("hr-admin")
        });
    }

    [HttpGet("test-auth")]
    [Authorize(Roles = "hr-admin")]
    public IActionResult TestAuthentication()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        var authInfo = new
        {
            IsAuthenticated = isAuthenticated,
            User.Identity?.AuthenticationType,
            Claims = isAuthenticated ? User.Claims.Select(c => new { c.Type, c.Value }).ToList() : null,
            UserId = isAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : "Not authenticated",
            Username = isAuthenticated ? User.FindFirstValue(ClaimTypes.Name) : "Not authenticated",
            IsAdmin = isAuthenticated && User.IsInRole("hr-admin")
        };

        logger.LogDebug("IsAuthenticated: {IsAuthenticated}", isAuthenticated);

        if (!isAuthenticated)
        {
            logger.LogDebug("AuthenticationType: {AuthenticationType}", User.Identity?.AuthenticationType);

            Console.WriteLine("User is not Authenticated");

            // Log the Authorization header to check if a token is being sent
            if (Request.Headers.ContainsKey("Authorization"))
            {
                var token = Request.Headers["Authorization"].ToString();
                logger.LogDebug("Received Token: {Token}", token);
            }
            else
            {
                logger.LogDebug("No Authorization Header Received");
            }

            foreach (var header in Request.Headers)
                logger.LogDebug("Header: {Key} = {Value}", header.Key, header.Value);
        }

        return Ok(new
        {
            Message = "Authentication test endpoint",
            AuthenticationInfo = authInfo,
            RequestHeaders = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        });
    }
}