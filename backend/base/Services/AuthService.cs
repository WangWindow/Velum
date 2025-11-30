using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Velum.Base.Data;
using Velum.Core.Enums;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Base.Services;

public class AuthService(ApplicationDbContext context, IConfiguration configuration) : IAuthService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IConfiguration _configuration = configuration;

    public async Task<(string? Token, User? User, string? Error)> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || user.PasswordHash != password)
        {
            return (null, null, "Invalid username or password");
        }

        var token = GenerateJwtToken(user);
        return (token, user, null);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(string username, string password, string? email, string? fullName, string? adminKey)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
        {
            return (false, "Username already exists");
        }

        var role = UserRoleType.User;
        if (!string.IsNullOrEmpty(adminKey))
        {
            var configuredAdminKey = _configuration["AdminSettings:RegistrationKey"];
            if (!string.IsNullOrEmpty(configuredAdminKey) && adminKey == configuredAdminKey)
            {
                role = UserRoleType.Admin;
            }
            else
            {
                return (false, "Invalid Admin Key");
            }
        }

        var user = new User
        {
            Username = username,
            PasswordHash = password, // In real app, hash this!
            Email = email,
            FullName = fullName,
            Role = role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim("role", user.Role.ToString().ToLowerInvariant()),
            new Claim("UserId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
          _configuration["Jwt:Audience"],
          claims,
          expires: DateTime.Now.AddMinutes(120),
          signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
