using LibroFiscal.Domain.Accounting.Entities;
using LibroFiscal.Domain.Common.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations.Accounting;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => AccountId.From(value));

        builder.Property(x => x.CompanyId)
            .HasConversion(id => id.Value, value => CompanyId.From(value))
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ParentAccountId)
            .HasConversion(id => id!.Value, value => AccountId.From(value));

        builder.HasOne(x => x.ParentAccount)
            .WithMany(x => x.SubAccounts)
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ensures unique Code per Company
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
    }
}
