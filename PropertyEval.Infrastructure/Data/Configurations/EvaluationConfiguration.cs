using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyEval.Domain.Entities;

namespace PropertyEval.Infrastructure.Data.Configurations;

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.ToTable("Evaluations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EvaluatedValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.EvaluationDate)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(e => e.Property)
            .WithMany(p => p.Evaluations)
            .HasForeignKey(e => e.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(u => u.Evaluations)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PropertyId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.EvaluationDate);
    }
}
