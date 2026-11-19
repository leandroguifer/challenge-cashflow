using Cashflow.Core.Data;
using Cashflow.Balance.Domain;
using Cashflow.Balance.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Balance.Persistence;

internal sealed class BalanceDbContext : CashflowDbContext
{
    public BalanceDbContext(DbContextOptions<BalanceDbContext> options) : base(options)
    {
    }

    public DbSet<DailyBalance> DailyBalances => Set<DailyBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DailyBalanceConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);

    }
}