using LibroFiscal.Domain.Users.Entities;
using LibroFiscal.Domain.Users.Enums;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.SharedKernel.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibroFiscal.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                userId => userId.Value,
                value => new UserId(value));

        builder.Property(u => u.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion(
                role => role.Id,
                value => Enumeration.FromId<UserRole>(value))
            .HasColumnName("RoleId")
            .IsRequired();

        builder.Property(u => u.IsActive).IsRequired();

        // Unique index on username
        builder.HasIndex(u => u.Username).IsUnique();

        // Seed data
        var adminId = new UserId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        builder.HasData(new
        {
            Id = adminId,
            Username = "admin",
            PasswordHash = "NtVzQj7YD3Kouyuh4SCM+g==:0+0tEzq+CqbcJfG8K774w0zA3UAK6liYeC3eAnZezfo=",
            Role = UserRole.Admin,
            IsActive = true
        });
    }
}
