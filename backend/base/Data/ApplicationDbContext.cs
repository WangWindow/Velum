using Microsoft.EntityFrameworkCore;
using Velum.Core.Enums;
using Velum.Core.Models;

namespace Velum.Base.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Questionnaire> Questionnaires { get; set; }
    public DbSet<UserTask> UserTasks { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Assessment> Assessments { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<UserRoleType>(v, true));

        modelBuilder.Entity<ChatMessage>()
            .Property(m => m.Role)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ChatRoleType>(v, true));
    }
}
