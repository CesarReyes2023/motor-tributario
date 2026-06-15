using LibroFiscal.Domain.Taxes.Entities;
using LibroFiscal.Domain.Taxes.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations;

public sealed class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        builder.ToTable("TaxRules");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Rate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(t => t.Type)
            .HasConversion(
                type => type.Id,
                value => LibroFiscal.SharedKernel.Primitives.Enumeration.FromId<TaxType>(value))
            .HasColumnName("TypeId")
            .IsRequired();

        builder.Property(t => t.IsActive).IsRequired();

        // Seed default tax rules for El Salvador
        builder.HasData(
            new
            {
                Id = Guid.Parse("A1A1A1A1-A1A1-A1A1-A1A1-A1A1A1A1A1A1"),
                Name = "Impuesto al Valor Agregado",
                Code = "IVA",
                Rate = 13.00m,
                Type = TaxType.Addition,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System"
            },
            new
            {
                Id = Guid.Parse("B2B2B2B2-B2B2-B2B2-B2B2-B2B2B2B2B2B2"),
                Name = "Retención Gran Contribuyente",
                Code = "RET_1",
                Rate = 1.00m,
                Type = TaxType.Deduction,
                IsActive = false,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System"
            }
        );
    }
}
