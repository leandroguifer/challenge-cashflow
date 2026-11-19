using Microsoft.EntityFrameworkCore;

namespace Cashflow.Core.Data;

/// <summary>Base DbContext of the services. Table and column names use snake_case.</summary>
/// <param name="options">The context options.</param>
public abstract class CashflowDbContext(DbContextOptions options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplySnakeCaseNames();
    }
}