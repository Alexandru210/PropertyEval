using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyEval.Domain.Entities;

namespace PropertyEval.Infrastructure.Data.Configurations;

public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.ToTable("PropertyImages");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.ImageUrl)
            .IsRequired()
            .HasMaxLength(2048)
            .IsUnicode(false);

        builder.Property(pi => pi.Description)
            .HasMaxLength(500);

        builder.Property(pi => pi.UploadedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(pi => pi.Property)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pi => pi.PropertyId);
    }
}
