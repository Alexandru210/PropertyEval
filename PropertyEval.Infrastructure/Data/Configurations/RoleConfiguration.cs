using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyEval.Domain.Constants;
using PropertyEval.Domain.Entities;

namespace PropertyEval.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private static readonly DateTime SeededAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.HasData(
            new Role
            {
                Id = SystemRoles.ClientId,
                Name = SystemRoles.Client,
                Description = "Default role for registered users.",
                CreatedAt = SeededAt
            },
            new Role
            {
                Id = SystemRoles.AdminId,
                Name = SystemRoles.Admin,
                Description = "Administrator role with full management access.",
                CreatedAt = SeededAt
            });
    }
}
