using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Velum.Base.Data;
using Velum.Core.Enums;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Base.Services;

public class AuthService(ApplicationDbContext context, IConfiguration configuration, IPasswordManager passwordManager) : IAuthService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IConfiguration _configuration = configuration;
    private readonly IPasswordManager _passwordManager = passwordManager;

    public async Task<(string? Token, User? User, string? Error)> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            return (null, null, "Invalid username or password");
        }

        try
        {
            if (!_passwordManager.VerifyPassword(password, user.Password))
            {
                return (null, null, "Invalid username or password");
            }
        }
        catch (Exception ex)
        {
            return (null, null, $"Authentication failed: {ex.Message}");
        }

        var token = GenerateJwtToken(user);
        return (token, user, null);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

        StringBuilder builder = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(string username, string password, string? email, string? fullName, string? registrationKey)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
        {
            return (false, "Username already exists");
        }

        // 校验注册码
        var requiredKey = _configuration["AdminSettings:RegistrationKey"];
        if (string.IsNullOrWhiteSpace(registrationKey) || registrationKey != requiredKey)
        {
            return (false, "Invalid or missing registration key");
        }

        // Decrypt password if encryption is enabled
        string plainPassword;
        try
        {
            plainPassword = _passwordManager.ProcessPassword(password);
        }
        catch (Exception ex)
        {
            return (false, $"Password processing failed: {ex.Message}");
        }

        var user = new User
        {
            Username = username,
            Password = plainPassword,
            Email = email,
            FullName = fullName ?? username,
            Role = UserRoleType.User,
            CreatedAt = DateTime.UtcNow
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
