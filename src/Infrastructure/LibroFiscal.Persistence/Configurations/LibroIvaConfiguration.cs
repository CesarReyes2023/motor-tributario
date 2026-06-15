using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.FiscalBooks.Entities;
using LibroFiscal.SharedKernel.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations;

public sealed class LibroIvaConfiguration : IEntityTypeConfiguration<LibroIva>
{
    public void Configure(EntityTypeBuilder<LibroIva> builder)
    {
        builder.ToTable("LibrosIva");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(
                libroId => libroId.Value,
                value => new LibroIvaId(value));

        builder.Property(l => l.CompanyId)
            .HasConversion(
                companyId => companyId.Value,
                value => new CompanyId(value))
            .IsRequired();

        // TipoLibroIva (Smart Enum)
        builder.Property(l => l.TipoLibro)
            .HasConversion(
                t => t.Id,
                value => Enumeration.FromId<TipoLibroIva>(value))
            .HasColumnName("TipoLibroId")
            .IsRequired();

        // EstadoLibroIva (Smart Enum)
        builder.Property(l => l.Estado)
            .HasConversion(
                e => e.Id,
                value => Enumeration.FromId<EstadoLibroIva>(value))
            .HasColumnName("EstadoId")
            .IsRequired();

        // Fiscal Period Value Object
        builder.OwnsOne(l => l.Periodo, periodBuilder =>
        {
            periodBuilder.Property(p => p.Year)
                .HasColumnName("FiscalYear")
                .IsRequired();
            
            periodBuilder.Property(p => p.Month)
                .HasColumnName("FiscalMonth")
                .IsRequired();
        });

        // Totals
        builder.Property(l => l.TotalGravado).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TotalExento).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TotalNoSujeto).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TotalIva).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TotalGeneral).HasColumnType("decimal(18,2)");

        // Entradas (One-to-Many as JSON or Owned)
        // For EF Core 8, we can map collections of ValueObjects to JSON columns
        builder.OwnsMany(l => l.Entradas, entradasBuilder =>
        {
            entradasBuilder.ToJson();
            entradasBuilder.Property(e => e.DteId).HasConversion(id => id.Value, value => new DteId(value));
        });

        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.CreatedBy).HasMaxLength(100);
        builder.Property(l => l.LastModifiedAt);
        builder.Property(l => l.LastModifiedBy).HasMaxLength(100);

        // Indices
        builder.HasIndex(l => new { l.CompanyId }).HasDatabaseName("IX_LibrosIva_CompanyId");
    }
}
