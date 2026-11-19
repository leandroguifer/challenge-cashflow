using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cashflow.Balance.Infrastructure.Persistence;

internal sealed class BalanceDbContextFactory : IDesignTimeDbContextFactory<BalanceDbContext>
{
    private const string FallbackConnectionString = "Host=localhost;Port=25432;Database=balance;Username=cashflow;Password=cashflow";
    public BalanceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Migrations") ?? FallbackConnectionString;
        var options = new DbContextOptionsBuilder<BalanceDbContext>().UseNpgsql(connectionString).Options;

        return new BalanceDbContext(options);
    }
}