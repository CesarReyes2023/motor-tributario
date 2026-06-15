using LibroFiscal.Domain.Purchases.Entities;
using LibroFiscal.Domain.Common.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations.Purchases;

internal sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => PurchaseId.From(value));

        builder.Property(x => x.CompanyId)
            .HasConversion(id => id.Value, value => CompanyId.From(value))
            .IsRequired();

        builder.Property(x => x.SupplierNit)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SupplierNrc)
            .HasMaxLength(20);

        builder.Property(x => x.SupplierName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DocumentNumber)
            .HasMaxLength(50);

        builder.Property(x => x.SubTotal)
            .HasPrecision(18, 4);

        builder.Property(x => x.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.JournalEntryId)
            .HasConversion(id => id!.Value, value => JournalEntryId.From(value));
    }
}
