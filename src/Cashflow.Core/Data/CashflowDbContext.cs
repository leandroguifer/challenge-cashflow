using Microsoft.EntityFrameworkCore;

namespace Cashflow.Core.Data;

public abstract class CashflowDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplySnakeCaseNames();
    }
}