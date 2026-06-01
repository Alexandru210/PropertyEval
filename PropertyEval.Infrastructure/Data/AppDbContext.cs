using Microsoft.EntityFrameworkCore;
using PropertyEval.Domain.Entities;
using PropertyEval.Infrastructure.Data.Configurations;

namespace PropertyEval.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<Property> Properties { get; set; } = null!;
    public DbSet<Listing> Listings { get; set; } = null!;
    public DbSet<Evaluation> Evaluations { get; set; } = null!;
    public DbSet<PropertyImage> PropertyImages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new PropertyConfiguration());
        modelBuilder.ApplyConfiguration(new ListingConfiguration());
        modelBuilder.ApplyConfiguration(new EvaluationConfiguration());
        modelBuilder.ApplyConfiguration(new PropertyImageConfiguration());
    }
}
