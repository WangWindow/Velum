using Microsoft.EntityFrameworkCore;
using Velum.Core.Enums;
using Velum.Core.Models;

namespace Velum.Base.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return; // DB has been seeded
        }

        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                PasswordHash = "admin123", // In real app, hash this
                Email = "admin@velum.com",
                FullName = "System Administrator",
                Role = UserRoleType.Admin,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "user",
                PasswordHash = "user123",
                Email = "user@velum.com",
                FullName = "Demo User",
                Role = UserRoleType.User,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}
