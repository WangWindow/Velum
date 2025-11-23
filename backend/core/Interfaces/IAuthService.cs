using Velum.Core.Enums;
using Velum.Core.Models;

namespace Velum.Core.Interfaces;

public interface IAuthService
{
    Task<(string? Token, User? User, string? Error)> LoginAsync(string username, string password);
    Task<(bool Success, string? Error)> RegisterAsync(string username, string password, string? email, string? fullName, string? adminKey);
}
