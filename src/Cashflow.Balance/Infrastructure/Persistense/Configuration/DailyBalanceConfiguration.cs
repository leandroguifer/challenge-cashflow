using Cashflow.Balance.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashflow.Balance.Infrastructure.Persistence.Configuration;

internal sealed class DailyBalanceConfiguration : IEntityTypeConfiguration<DailyBalance>
{
    public void Configure(EntityTypeBuilder<DailyBalance> builder)
    {
        builder.HasKey(e => new {e.StoreId, e.BusinessDate });
        
        builder.Property(e => e.StoreId).HasMaxLength(50).IsRequired();
        builder.Property(e => e.BusinessDate).IsRequired();
        builder.Property(e => e.TotalCredits).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.TotalDebits).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.EntriesCount).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.Ignore(e => e.TotalBalance);
        
    }
}