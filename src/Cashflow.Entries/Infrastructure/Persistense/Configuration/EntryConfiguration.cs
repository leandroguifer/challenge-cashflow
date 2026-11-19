using Cashflow.Entries.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashflow.Entries.Infrastructure.Persistence.Configuration;

internal sealed class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CashRegisterId).IsRequired();
        builder.Property(e => e.StoreId).IsRequired();
        builder.Property(e => e.BusinessDate).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Amount).IsRequired();
        builder.Property(e => e.Description);
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasOne<CashRegister>()
            .WithMany()
            .HasForeignKey(e => e.CashRegisterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}