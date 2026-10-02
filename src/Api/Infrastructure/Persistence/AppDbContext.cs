using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<WeatherNote> WeatherNotes => Set<WeatherNote>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Country>(e =>
        {
            e.HasKey(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(2);
        });
        b.Entity<City>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Name, x.CountryCode }).IsUnique();
        });
        b.Entity<WeatherNote>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Note).HasMaxLength(500);
        });
    }
}
