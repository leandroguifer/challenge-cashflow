using Microsoft.EntityFrameworkCore;

namespace Cashflow.Entries.Infrastructure.Persistence;

internal sealed class EntriesMigrationService : IHostedService
{
    private readonly string _connectionString;
    public EntriesMigrationService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Migrations") ?? string.Empty;
        ArgumentNullException.ThrowIfNull(_connectionString, "Connection string 'Migrations' not found.");
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<EntriesDbContext>().UseNpgsql(_connectionString).Options;
        await using var context = new EntriesDbContext(options);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
}