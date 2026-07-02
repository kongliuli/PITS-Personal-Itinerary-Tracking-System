using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using PITS.MVP.Core.Entities;

namespace PITS.MVP.Infrastructure.Data;

public class TripContext : DbContext
{
    private static readonly ValueConverter<Point?, byte[]?> NullablePointConverter =
        new(point => WritePoint(point), bytes => ReadPoint(bytes));

    private static readonly ValueConverter<Point, byte[]> PointConverter =
        new(point => WritePoint(point)!, bytes => ReadPoint(bytes)!);

    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripPlan> TripPlans => Set<TripPlan>();
    public DbSet<Place> Places => Set<Place>();
    public DbSet<TrackPoint> TrackPoints => Set<TrackPoint>();
    public DbSet<ImportStagingItem> ImportStagingItems => Set<ImportStagingItem>();

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
            entity.HasIndex(e => e.PlanId);

            entity.Property(e => e.ActivityType).HasConversion<string>();
            entity.Property(e => e.Visibility).HasConversion<string>();
            entity.Property(e => e.Source).HasConversion<string>();
            entity.Property(e => e.Location).HasConversion(NullablePointConverter).HasColumnType("BLOB");

            entity.HasOne(e => e.Plan)
                .WithMany(e => e.ActualTrips)
                .HasForeignKey(e => e.PlanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        model.Entity<TripPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartsAt);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.ExternalId);
            entity.Property(e => e.ActivityType).HasConversion<string>();
            entity.Property(e => e.Visibility).HasConversion<string>();
            entity.Property(e => e.Source).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Location).HasConversion(NullablePointConverter).HasColumnType("BLOB");
        });

        model.Entity<Place>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GeoHash);
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.Category).HasConversion<string>();
            entity.Property(e => e.Location).HasConversion(NullablePointConverter).HasColumnType("BLOB");
        });

        model.Entity<TrackPoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.TripId);
            entity.Property(e => e.Location).HasConversion(PointConverter).HasColumnType("BLOB");
        });

        model.Entity<ImportStagingItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Fingerprint).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartsAt);
            entity.Property(e => e.Source).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Location).HasConversion(NullablePointConverter).HasColumnType("BLOB");
        });
    }

    private static byte[]? WritePoint(Point? point)
    {
        return point == null ? null : new WKBWriter().Write(point);
    }

    private static Point? ReadPoint(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        var point = (Point)new WKBReader().Read(bytes);
        if (point.SRID == 0) point.SRID = 4326;
        return point;
    }
}
