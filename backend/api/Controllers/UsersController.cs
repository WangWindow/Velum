using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Velum.Base.Data;
using Velum.Base.Services;
using Velum.Core.Enums;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController(ApplicationDbContext context, ILogService logService) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogService _logService = logService;

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers([FromQuery] UserRoleType? role)
    {
        var query = _context.Users.AsQueryable();
        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }
        return await query.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
    {
        if (_context.Users.Any(u => u.Username == request.Username))
        {
            return BadRequest("Username already exists");
        }

        var user = new User
        {
            Username = request.Username,
            Password = request.Password,
            Email = request.Email,
            FullName = request.FullName,
            Role = Enum.Parse<UserRoleType>(request.Role, true)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _logService.LogInfoAsync(
            message: $"User created: {user.Username} by Admin",
            userId: GetCurrentUserId(),
            action: "CreateUser",
            resource: "Users"
        );

        return CreatedAtAction("GetUser", new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }

        existingUser.Email = request.Email ?? existingUser.Email;
        existingUser.FullName = request.FullName ?? existingUser.FullName;
        existingUser.Avatar = request.Avatar ?? existingUser.Avatar;

        if (!string.IsNullOrEmpty(request.Role))
        {
            existingUser.Role = Enum.Parse<UserRoleType>(request.Role, true);
        }

        if (!string.IsNullOrEmpty(request.Password))
        {
            existingUser.Password = request.Password; // In real app, hash this!
        }

        try
        {
            await _context.SaveChangesAsync();

            await _logService.LogInfoAsync(
                message: $"User updated: {existingUser.Username} by Admin",
                userId: GetCurrentUserId(),
                action: "UpdateUser",
                resource: "Users"
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.Username == "admin")
        {
            return BadRequest("Cannot delete the main administrator.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        await _logService.LogInfoAsync(
            message: $"User deleted: {user.Username} by Admin",
            userId: GetCurrentUserId(),
            action: "DeleteUser",
            resource: "Users"
        );

        return NoContent();
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateUserRequest request)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var id = int.Parse(userIdClaim.Value);

        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }

        existingUser.Email = request.Email ?? existingUser.Email;
        existingUser.FullName = request.FullName ?? existingUser.FullName;
        existingUser.Avatar = request.Avatar ?? existingUser.Avatar;

        if (!string.IsNullOrEmpty(request.Password))
        {
            existingUser.Password = request.Password; // In real app, hash this!
        }

        // Users cannot update their own role via this endpoint

        try
        {
            await _context.SaveChangesAsync();

            await _logService.LogInfoAsync(
                message: $"User updated profile: {existingUser.Username}",
                userId: id,
                action: "UpdateProfile",
                resource: "Users"
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }
}

public class CreateUserRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string Role { get; set; } = "User";
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string? Password { get; set; }
    public string? Avatar { get; set; }
}
