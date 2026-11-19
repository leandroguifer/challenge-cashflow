using Microsoft.EntityFrameworkCore;

namespace Cashflow.Entries.Infrastructure.Persistence;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddEntriesPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        ArgumentNullException.ThrowIfNull(connectionString, "Connection string 'Default' not found.");
        
        services.AddDbContext<EntriesDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}