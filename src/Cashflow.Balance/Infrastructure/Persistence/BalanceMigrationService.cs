using Microsoft.EntityFrameworkCore;

namespace Cashflow.Balance.Infrastructure.Persistence;

internal sealed class BalanceMigrationService : IHostedService
{
    private readonly string _connectionString;
    public BalanceMigrationService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Migrations") ?? string.Empty;
        ArgumentNullException.ThrowIfNull(_connectionString, "Connection string 'Migrations' not found.");
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<BalanceDbContext>().UseNpgsql(_connectionString).Options;
        await using var context = new BalanceDbContext(options);
        await context.Database.MigrateAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
}