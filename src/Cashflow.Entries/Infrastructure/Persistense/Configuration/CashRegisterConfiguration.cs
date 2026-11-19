using Cashflow.Entries.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashflow.Entries.Persistence.Configuration;

internal sealed class CashRegisterConfiguration : IEntityTypeConfiguration<CashRegister>
{
    public void Configure(EntityTypeBuilder<CashRegister> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.StoreId).IsRequired();
        builder.Property(e => e.BusinessDate).IsRequired();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.OpenedBy).IsRequired();
        builder.Property(e => e.OpenedAt).IsRequired();
        builder.Property(e => e.ClosedBy);
        builder.Property(e => e.ClosedAt);
    }
}