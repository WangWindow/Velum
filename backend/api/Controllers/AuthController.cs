using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Velum.Base.Data;
using Velum.Base.Services;
using Velum.Core.Enums;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, ILogService logService) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ILogService _logService = logService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        Console.WriteLine($"Login attempt for user: {model.Username}");
        var (token, user, error) = await _authService.LoginAsync(model.Username, model.Password);

        if (error != null)
        {
            Console.WriteLine(error);
            await _logService.LogWarningAsync(
                message: $"Failed login attempt for username: {model.Username}. Reason: {error}",
                action: "Login",
                resource: "Auth"
            );
            return Unauthorized(new MessageResponse { Message = error });
        }

        Console.WriteLine("Login successful");

        await _logService.LogInfoAsync(
            message: $"User logged in: {user!.Username}",
            userId: user.Id,
            action: "Login",
            resource: "Auth"
        );

        return Ok(new LoginResponse
        {
            Token = token!,
            Role = user!.Role,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Avatar = user.Avatar
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var (success, error) = await _authService.RegisterAsync(model.Username, model.Password, model.Email, model.FullName, model.AdminKey);

        if (!success)
        {
            await _logService.LogWarningAsync(
                message: $"Failed registration attempt for username: {model.Username}. Reason: {error}",
                action: "Register",
                resource: "Auth"
            );
            return BadRequest(error);
        }

        await _logService.LogInfoAsync(
            message: $"New user registered: {model.Username}",
            action: "Register",
            resource: "Auth"
        );

        return Ok(new MessageResponse { Message = "User registered successfully" });
    }
}


public class LoginModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class RegisterModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? AdminKey { get; set; }
}

public class LoginResponse
{
    public required string Token { get; set; }
    public required UserRoleType Role { get; set; }
    public required UserDto User { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
}

public class MessageResponse
{
    public required string Message { get; set; }
}

public class AuthResponse
{
    public required string Token { get; set; }
    public required User User { get; set; }
}

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class RegisterRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? AdminKey { get; set; }
}
