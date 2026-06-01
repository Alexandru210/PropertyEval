using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyEval.Domain.Entities;

namespace PropertyEval.Infrastructure.Data.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        // Table name
        builder.ToTable("Listings");

        // Primary Key
        builder.HasKey(l => l.Id);

        // Properties
        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.AskingPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(l => l.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasOne(l => l.Property)
            .WithMany(p => p.Listings)
            .HasForeignKey(l => l.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.User)
            .WithMany(u => u.Listings)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(l => l.PropertyId);

        builder.HasIndex(l => l.UserId);

        builder.HasIndex(l => l.Status);

        builder.HasIndex(l => l.AskingPrice);

        builder.HasIndex(l => new { l.Status, l.AskingPrice });
    }
}
