using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Velum.Core.Models;
using Velum.Base.Data;

namespace Velum.Base.Configuration;

public class DatabaseConfigurationSource(Action<DbContextOptionsBuilder> optionsAction) : IConfigurationSource
{
    private readonly Action<DbContextOptionsBuilder> _optionsAction = optionsAction;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DatabaseConfigurationProvider(_optionsAction);
    }
}

public class DatabaseConfigurationProvider(Action<DbContextOptionsBuilder> optionsAction) : ConfigurationProvider
{
    private readonly Action<DbContextOptionsBuilder> _optionsAction = optionsAction;

    public override void Load()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        _optionsAction(builder);

        using var context = new ApplicationDbContext(builder.Options);

        // Ensure database is created/migrated if we are going to read from it.
        // However, in a real production scenario, we might not want to migrate here implicitly.
        // For now, we'll try to just read. If the table doesn't exist, we might catch exception or just ignore.
        try
        {
            if (context.Database.CanConnect())
            {
                // Check if table exists to avoid crashing on first run before migrations
                // This is a bit specific to provider, but for SQLite/EF Core:
                try
                {
                    Data = context.AppSettings.ToDictionary(c => c.Key, c => (string?)c.Value, StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                    // Table might not exist yet
                }
            }
        }
        catch
        {
            // Database might not exist yet
        }
    }
}
