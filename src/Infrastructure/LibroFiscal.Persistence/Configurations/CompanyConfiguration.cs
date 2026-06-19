using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.SharedKernel.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                companyId => companyId.Value,
                value => new CompanyId(value));

        builder.Property(c => c.RazonSocial)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.NombreComercial)
            .HasMaxLength(200);

        builder.Property(c => c.CodigoActividad)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.DescripcionActividad)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Telefono)
            .HasMaxLength(20);

        builder.Property(c => c.Correo)
            .HasMaxLength(100);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.ApiPassword)
            .HasMaxLength(255)
            .HasDefaultValue("");

        builder.Property(c => c.LogoPath)
            .HasMaxLength(500)
            .IsRequired(false);

        // Owns NIT as a value object
        builder.OwnsOne(c => c.Nit, nitBuilder =>
        {
            nitBuilder.Property(n => n.Value)
                .HasColumnName("Nit")
                .HasMaxLength(14)
                .IsRequired();
            // Don't map the Formatted property
            nitBuilder.Ignore(n => n.Formatted);
        });

        // Owns NRC as a value object
        builder.OwnsOne(c => c.Nrc, nrcBuilder =>
        {
            nrcBuilder.Property(n => n.Value)
                .HasColumnName("Nrc")
                .HasMaxLength(10)
                .IsRequired();
            nrcBuilder.Ignore(n => n.Formatted);
        });

        // Direccion Fiscal
        builder.OwnsOne(c => c.DireccionFiscal, dirBuilder =>
        {
            dirBuilder.Property(d => d.Departamento).HasColumnName("Departamento").HasMaxLength(100).IsRequired();
            dirBuilder.Property(d => d.Municipio).HasColumnName("Municipio").HasMaxLength(100).IsRequired();
            dirBuilder.Property(d => d.Complemento).HasColumnName("DireccionComplemento").HasMaxLength(200).IsRequired();
        });

        // Ambiente Hacienda
        builder.Property(c => c.Ambiente)
            .HasConversion(
                a => a.Id,
                value => Enumeration.FromId<AmbienteHacienda>(value))
            .HasColumnName("AmbienteId")
            .IsRequired();

        // Establishments relationship (One-to-Many instead of OwnsMany since it's an Entity)
        builder.HasMany(c => c.Establishments)
            .WithOne()
            .HasForeignKey(e => e.CompanyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Audit fields
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(100);
        builder.Property(c => c.LastModifiedAt);
        builder.Property(c => c.LastModifiedBy).HasMaxLength(100);
    }
}

public sealed class EstablishmentConfiguration : IEntityTypeConfiguration<Establishment>
{
    public void Configure(EntityTypeBuilder<Establishment> builder)
    {
        builder.ToTable("Establishments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                establishmentId => establishmentId.Value,
                value => new EstablishmentId(value));

        builder.Property(e => e.Codigo)
            .HasMaxLength(4)
            .IsRequired();

        builder.Property(e => e.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(e => e.PuntoVenta)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.Telefono)
            .HasMaxLength(20);

        builder.Property(e => e.Correo)
            .HasMaxLength(100);

        builder.OwnsOne(e => e.Direccion, dirBuilder =>
        {
            dirBuilder.Property(d => d.Departamento).HasColumnName("Departamento").HasMaxLength(100).IsRequired();
            dirBuilder.Property(d => d.Municipio).HasColumnName("Municipio").HasMaxLength(100).IsRequired();
            dirBuilder.Property(d => d.Complemento).HasColumnName("DireccionComplemento").HasMaxLength(200).IsRequired();
        });
    }
}
