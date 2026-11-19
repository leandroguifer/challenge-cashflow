using Microsoft.EntityFrameworkCore;

namespace Cashflow.Balance.Infrastructure.Persistence;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        ArgumentNullException.ThrowIfNull(connectionString, "Connection string 'Default' not found.");
        
        services.AddDbContext<BalanceDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}