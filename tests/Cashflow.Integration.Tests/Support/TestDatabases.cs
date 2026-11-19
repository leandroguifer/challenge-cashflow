using Cashflow.Balance.Infrastructure.Persistence;
using Cashflow.Entries.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Integration.Tests.Support;

internal sealed class TestDatabases
{
    private readonly InfrastructureFixture infrastructure;

    private TestDatabases(InfrastructureFixture infrastructure, string entries, string balance)
    {
        this.infrastructure = infrastructure;
        Entries = entries;
        Balance = balance;
    }

    public string Entries { get; }

    public string Balance { get; }

    public string EntriesMigrations => infrastructure.SuperConnection(Entries);

    public string BalanceMigrations => infrastructure.SuperConnection(Balance);

    public string EntriesAs(string role) => infrastructure.RoleConnection(Entries, role);

    public string BalanceAs(string role) => infrastructure.RoleConnection(Balance, role);

    public static async Task<TestDatabases> CreateAsync(InfrastructureFixture infrastructure)
    {
        infrastructure.RequireAvailable();

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var databases = new TestDatabases(infrastructure, $"entries_{suffix}", $"balance_{suffix}");

        await using (var entries = new EntriesDbContext(new DbContextOptionsBuilder<EntriesDbContext>().UseNpgsql(databases.EntriesMigrations).Options))
        {
            await entries.Database.MigrateAsync();
        }

        await using (var balance = new BalanceDbContext(new DbContextOptionsBuilder<BalanceDbContext>().UseNpgsql(databases.BalanceMigrations).Options))
        {
            await balance.Database.MigrateAsync();
        }

        return databases;
    }
}
