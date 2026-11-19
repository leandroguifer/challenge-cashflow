using Cashflow.Core.Data;
using Cashflow.Entries.Domain;
using Cashflow.Entries.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Entries.Persistence;

internal sealed class EntriesDbContext : CashflowDbContext
{
    public EntriesDbContext(DbContextOptions<EntriesDbContext> options) : base(options)
    {
    }

    public DbSet<CashRegister> CashRegisters  => Set<CashRegister>();
    public DbSet<Entry> Entries  => Set<Entry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashRegisterConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntryConfiguration).Assembly);

        base.OnModelCreating(modelBuilder);

    }
}