using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations;

public sealed class UserCompanyAccessConfiguration : IEntityTypeConfiguration<UserCompanyAccess>
{
    public void Configure(EntityTypeBuilder<UserCompanyAccess> builder)
    {
        builder.ToTable("UserCompanyAccesses");

        builder.HasKey(uca => new { uca.UserId, uca.CompanyId });

        builder.Property(uca => uca.UserId)
            .HasConversion(
                userId => userId.Value,
                value => new LibroFiscal.Domain.Users.Ids.UserId(value))
            .IsRequired();

        builder.Property(uca => uca.CompanyId)
            .HasConversion(
                companyId => companyId.Value,
                value => new CompanyId(value))
            .IsRequired();
            
        builder.HasOne<User>()
            .WithMany(u => u.CompanyAccesses)
            .HasForeignKey(uca => uca.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
