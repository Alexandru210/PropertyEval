using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyEval.Domain.Constants;
using PropertyEval.Domain.Entities;

namespace PropertyEval.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table name
        builder.ToTable("Users");

        // Primary Key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(u => u.RoleId)
            .IsRequired()
            .HasDefaultValue(SystemRoles.ClientId);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.RoleId);
    }
}
