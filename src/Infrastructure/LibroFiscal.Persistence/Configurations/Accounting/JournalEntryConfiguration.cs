using LibroFiscal.Domain.Accounting.Entities;
using LibroFiscal.Domain.Common.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations.Accounting;

internal sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => JournalEntryId.From(value));

        builder.Property(x => x.CompanyId)
            .HasConversion(id => id.Value, value => CompanyId.From(value))
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ReferenceDocumentId)
            .HasMaxLength(100);

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.JournalEntryId)
            .HasConversion(id => id.Value, value => JournalEntryId.From(value))
            .IsRequired();

        builder.Property(x => x.AccountId)
            .HasConversion(id => id.Value, value => AccountId.From(value))
            .IsRequired();

        builder.Property(x => x.Debit)
            .HasPrecision(18, 4);

        builder.Property(x => x.Credit)
            .HasPrecision(18, 4);
    }
}
