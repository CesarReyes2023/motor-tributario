using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.Domain.Common.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations.Companies;

internal sealed class InvoiceTemplateConfiguration : IEntityTypeConfiguration<InvoiceTemplate>
{
    public void Configure(EntityTypeBuilder<InvoiceTemplate> builder)
    {
        builder.ToTable("InvoiceTemplates");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => InvoiceTemplateId.From(value));

        builder.Property(x => x.CompanyId)
            .HasConversion(id => id.Value, value => CompanyId.From(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Use nvarchar(max) for HTML content
        builder.Property(x => x.HtmlContent)
            .IsRequired();
    }
}
