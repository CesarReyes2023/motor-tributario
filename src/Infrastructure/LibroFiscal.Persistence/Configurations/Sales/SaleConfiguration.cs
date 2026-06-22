using LibroFiscal.Domain.Sales.Entities;
using LibroFiscal.Domain.Common.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations.Sales;

internal sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => SaleId.From(value));

        builder.Property(x => x.CompanyId)
            .HasConversion(id => id.Value, value => CompanyId.From(value))
            .IsRequired();

        builder.Property(x => x.CustomerNit)
            .HasMaxLength(20);

        builder.Property(x => x.CustomerNrc)
            .HasMaxLength(20);

        builder.Property(x => x.CustomerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DocumentNumber)
            .HasMaxLength(50);

        builder.Property(x => x.TaxableAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.ExemptAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.JournalEntryId)
            .HasConversion(id => id!.Value, value => JournalEntryId.From(value));
    }
}
