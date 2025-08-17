using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyEval.Domain.Entities;

namespace PropertyEval.Infrastructure.Data.Configurations
{
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

            builder.Property(l => l.Status)
                .IsRequired();

            builder.Property(l => l.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
