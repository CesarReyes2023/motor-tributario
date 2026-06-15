using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.Domain.DTE.Events;
using LibroFiscal.SharedKernel.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations;

public sealed class DteDocumentConfiguration : IEntityTypeConfiguration<DteDocument>
{
    public void Configure(EntityTypeBuilder<DteDocument> builder)
    {
        builder.ToTable("Dtes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasConversion(
                dteId => dteId.Value,
                value => new DteId(value));

        // CompanyId mapping for Multi-tenancy
        builder.Property(d => d.CompanyId)
            .HasConversion(
                companyId => companyId.Value,
                value => new CompanyId(value))
            .IsRequired();

        // Enums
        builder.Property(d => d.Ambiente)
            .HasConversion(
                a => a.Id,
                value => Enumeration.FromId<AmbienteHacienda>(value))
            .HasColumnName("AmbienteId")
            .IsRequired();

        builder.Property(d => d.TipoDte)
            .HasConversion(
                t => t.Id,
                value => Enumeration.FromId<TipoDte>(value))
            .HasColumnName("TipoDteId")
            .IsRequired();

        builder.Property(d => d.Estado)
            .HasConversion(
                s => s.Id,
                value => Enumeration.FromId<EstadoDte>(value))
            .HasColumnName("EstadoId")
            .IsRequired();

        builder.Property(d => d.ModeloFacturacion)
            .HasConversion(
                m => m.Id,
                value => Enumeration.FromId<ModeloFacturacion>(value))
            .HasColumnName("ModeloFacturacionId")
            .IsRequired();

        builder.Property(d => d.TipoTransmision)
            .HasConversion(
                t => t.Id,
                value => Enumeration.FromId<TipoTransmision>(value))
            .HasColumnName("TipoTransmisionId")
            .IsRequired();

        // NumeroControl (Value Object)
        builder.OwnsOne(d => d.NumeroControl, ncBuilder =>
        {
            ncBuilder.Property(n => n.Value)
                .HasColumnName("NumeroControl")
                .HasMaxLength(31);
        });

        builder.Property(d => d.CodigoGeneracion)
            .HasMaxLength(36);

        builder.Property(d => d.SelloRecepcion)
            .HasMaxLength(40);

        builder.Property(d => d.FechaEmision)
            .IsRequired();

        // JSON Columns mapped from Value Objects using EF Core 8 ToJson()
        builder.OwnsOne(d => d.Emisor, eb =>
        {
            eb.ToJson();
            eb.OwnsOne(e => e.Direccion);
        });
        builder.OwnsOne(d => d.Receptor, rb =>
        {
            rb.ToJson();
            rb.OwnsOne(r => r.Direccion);
        });
        builder.OwnsOne(d => d.Resumen, rb =>
        {
            rb.ToJson();
            rb.Property(r => r.CondicionOperacion)
              .HasConversion(
                  c => c.Id,
                  value => Enumeration.FromId<CondicionOperacion>(value));
        });
        builder.OwnsMany(d => d.CuerpoDocumento, cb =>
        {
            cb.ToJson();
            cb.Property(c => c.TipoImpuesto)
              .HasConversion(
                  t => t.Id,
                  value => Enumeration.FromId<TipoImpuesto>(value));
        });
        builder.OwnsMany(d => d.HistorialEstados, hb =>
        {
            hb.ToJson();
            hb.Property(h => h.Estado)
              .HasConversion(
                  e => e.Id,
                  value => Enumeration.FromId<EstadoDte>(value));
        });

        // Tracking fields
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.CreatedBy).HasMaxLength(100);
        builder.Property(d => d.LastModifiedAt);
        builder.Property(d => d.LastModifiedBy).HasMaxLength(100);

        // Indices
        builder.HasIndex(d => d.CompanyId);
        builder.HasIndex(d => d.CodigoGeneracion).IsUnique().HasFilter("\"CodigoGeneracion\" IS NOT NULL");
        builder.HasIndex(d => new { d.CompanyId, d.Estado });
    }
}
