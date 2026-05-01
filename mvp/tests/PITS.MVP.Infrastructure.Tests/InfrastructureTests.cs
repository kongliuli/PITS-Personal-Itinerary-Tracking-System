using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Infrastructure.Data;
using Xunit;

namespace PITS.MVP.Infrastructure.Tests;

public class TripContextTests : IDisposable
{
    private readonly TripContext _context;

    public TripContextTests()
    {
        var options = new DbContextOptionsBuilder<TripContext>()
            .UseSqlite($"DataSource=:memory:")
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TripContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public void Database_CanBeCreated()
    {
        Assert.True(_context.Database.CanConnect());
    }

    [Fact]
    public async Task Trip_CanBeAddedAndRetrieved()
    {
        var trip = new Trip
        {
            StartedAt = DateTime.UtcNow,
            ActivityType = ActivityType.Work,
            Description = "Test trip"
        };

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Trips.FindAsync(trip.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test trip", retrieved.Description);
    }

    [Fact]
    public async Task Trip_WithLocation_CanBeSaved()
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var point = geometryFactory.CreatePoint(new Coordinate(121.4737, 31.2304));

        var trip = new Trip
        {
            StartedAt = DateTime.UtcNow,
            ActivityType = ActivityType.Work,
            Location = point,
            GeoHash = "wtw3s0gf"
        };

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Trips.FindAsync(trip.Id);
        Assert.NotNull(retrieved);
        Assert.NotNull(retrieved.Location);
        Assert.Equal(121.4737, retrieved.Location.X, 4);
        Assert.Equal(31.2304, retrieved.Location.Y, 4);
    }

    [Fact]
    public async Task Trip_CanBeUpdated()
    {
        var trip = new Trip
        {
            StartedAt = DateTime.UtcNow,
            ActivityType = ActivityType.Work,
            Description = "Original"
        };

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        trip.Description = "Updated";
        await _context.SaveChangesAsync();

        var retrieved = await _context.Trips.FindAsync(trip.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Updated", retrieved.Description);
    }

    [Fact]
    public async Task Trip_CanBeDeleted()
    {
        var trip = new Trip
        {
            StartedAt = DateTime.UtcNow,
            ActivityType = ActivityType.Work
        };

        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Trips.FindAsync(trip.Id);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task Place_CanBeAddedAndRetrieved()
    {
        var place = new Place
        {
            Name = "Test Office",
            Category = PlaceCategory.Office
        };

        _context.Places.Add(place);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Places.FindAsync(place.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Office", retrieved.Name);
    }

    [Fact]
    public async Task Trip_QueryByDateRange_ReturnsCorrectResults()
    {
        var today = DateTime.Today;
        var trips = new[]
        {
            new Trip { StartedAt = today.AddHours(9), ActivityType = ActivityType.Work },
            new Trip { StartedAt = today.AddHours(14), ActivityType = ActivityType.Personal },
            new Trip { StartedAt = today.AddDays(-1).AddHours(10), ActivityType = ActivityType.Work }
        };

        _context.Trips.AddRange(trips);
        await _context.SaveChangesAsync();

        var result = await _context.Trips
            .Where(t => t.StartedAt >= today && t.StartedAt < today.AddDays(1))
            .ToListAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Trip_QueryByActivityType_ReturnsCorrectResults()
    {
        _context.Trips.AddRange(
            new Trip { StartedAt = DateTime.UtcNow, ActivityType = ActivityType.Work },
            new Trip { StartedAt = DateTime.UtcNow, ActivityType = ActivityType.Work },
            new Trip { StartedAt = DateTime.UtcNow, ActivityType = ActivityType.Personal }
        );
        await _context.SaveChangesAsync();

        var result = await _context.Trips
            .Where(t => t.ActivityType == ActivityType.Work)
            .ToListAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task TrackPoint_CanBeAddedAndRetrieved()
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var point = geometryFactory.CreatePoint(new Coordinate(121.4737, 31.2304));

        var trackPoint = new TrackPoint
        {
            Timestamp = DateTime.UtcNow,
            Location = point,
            Accuracy = 5.0,
            Speed = 10.0
        };

        _context.TrackPoints.Add(trackPoint);
        await _context.SaveChangesAsync();

        var retrieved = await _context.TrackPoints.FindAsync(trackPoint.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(5.0, retrieved.Accuracy);
    }

    [Fact]
    public async Task MultipleTrips_CanBeAdded()
    {
        var trips = Enumerable.Range(0, 100).Select(i => new Trip
        {
            StartedAt = DateTime.UtcNow.AddHours(-i),
            ActivityType = ActivityType.Work,
            Description = $"Trip {i}"
        });

        _context.Trips.AddRange(trips);
        await _context.SaveChangesAsync();

        var count = await _context.Trips.CountAsync();
        Assert.Equal(100, count);
    }
}
