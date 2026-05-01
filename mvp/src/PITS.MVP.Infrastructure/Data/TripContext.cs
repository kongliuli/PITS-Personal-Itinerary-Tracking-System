using Microsoft.EntityFrameworkCore;
using PITS.MVP.Core.Entities;

namespace PITS.MVP.Infrastructure.Data;

public class TripContext : DbContext
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Place> Places => Set<Place>();
    public DbSet<TrackPoint> TrackPoints => Set<TrackPoint>();

    public TripContext(DbContextOptions<TripContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.ActivityType);
            entity.HasIndex(e => e.Visibility);
            entity.HasIndex(e => e.GeoHash);
            entity.HasIndex(e => e.PlaceId);

            entity.Property(e => e.ActivityType).HasConversion<string>();
            entity.Property(e => e.Visibility).HasConversion<string>();
            entity.Property(e => e.Source).HasConversion<string>();
        });

        model.Entity<Place>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GeoHash);
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.Category).HasConversion<string>();
        });

        model.Entity<TrackPoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.TripId);
        });
    }
}
