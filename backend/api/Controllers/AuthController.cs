using Microsoft.AspNetCore.Mvc;
using Velum.Core.Enums;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        Console.WriteLine($"Login attempt for user: {model.Username}");
        var (token, user, error) = await _authService.LoginAsync(model.Username, model.Password);

        if (error != null)
        {
            Console.WriteLine(error);
            return Unauthorized(new MessageResponse { Message = error });
        }

        Console.WriteLine("Login successful");
        return Ok(new LoginResponse
        {
            Token = token!,
            Role = user!.Role,
            User = new UserDto
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var (success, error) = await _authService.RegisterAsync(model.Username, model.Password, model.Email, model.FullName, model.AdminKey);

        if (!success)
        {
            return BadRequest(error);
        }

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
    public required string Username { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}

public class MessageResponse
{
    public required string Message { get; set; }
}
