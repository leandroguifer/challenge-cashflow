using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cashflow.Entries.Infrastructure.Persistence;

internal sealed class EntriesDbContextFactory : IDesignTimeDbContextFactory<EntriesDbContext>
{
    private const string FallbackConnectionString = "Host=localhost;Port=25432;Database=entries;Username=cashflow;Password=cashflow";
    public EntriesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Migrations") ?? FallbackConnectionString;
        var options = new DbContextOptionsBuilder<EntriesDbContext>().UseNpgsql(connectionString).Options;

        return new EntriesDbContext(options);
    }
}