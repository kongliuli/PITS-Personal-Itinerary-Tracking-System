using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Infrastructure.Data;
using PITS.MVP.Infrastructure.Services;
using Xunit;

namespace PITS.MVP.Infrastructure.Tests;

public class TripContextTests : IDisposable
{
    private readonly TripContext _context;

    public TripContextTests()
    {
        var options = new DbContextOptionsBuilder<TripContext>()
            .UseSqlite($"DataSource=:memory:", sqliteOptions => sqliteOptions.UseNetTopologySuite())
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

public class TransportModeDetectorTests
{
    private readonly TransportModeDetector _detector = new();
    private readonly GeometryFactory _geometryFactory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    private TrackPoint CreatePoint(double lon, double lat, DateTime timestamp)
    {
        return new TrackPoint
        {
            Location = _geometryFactory.CreatePoint(new Coordinate(lon, lat)),
            Timestamp = timestamp
        };
    }

    [Fact]
    public void DetectMode_NullInput_ReturnsOther()
    {
        var result = _detector.DetectMode(null!);
        Assert.Equal(ActivityType.Other, result.Mode);
        Assert.Equal(0, result.Confidence);
    }

    [Fact]
    public void DetectMode_EmptyInput_ReturnsOther()
    {
        var result = _detector.DetectMode(Array.Empty<TrackPoint>());
        Assert.Equal(ActivityType.Other, result.Mode);
        Assert.Equal(0, result.Confidence);
    }

    [Fact]
    public void DetectMode_SinglePoint_ReturnsOther()
    {
        var points = new[] { CreatePoint(121.4737, 31.2304, DateTime.UtcNow) };
        var result = _detector.DetectMode(points);
        Assert.Equal(ActivityType.Other, result.Mode);
    }

    [Fact]
    public void DetectMode_WalkingSpeed_DetectedAsWalking()
    {
        // 步行速度约 5 km/h ≈ 1.39 m/s
        // 每秒移动约 0.0000125 度纬度
        var baseTime = DateTime.UtcNow;
        var points = new List<TrackPoint>();
        for (int i = 0; i < 10; i++)
        {
            points.Add(CreatePoint(121.4737, 31.2304 + i * 0.0000125, baseTime.AddSeconds(i)));
        }

        var result = _detector.DetectMode(points);
        Assert.Equal(ActivityType.Walking, result.Mode);
        Assert.True(result.AverageSpeedKmh > 0);
        Assert.True(result.AverageSpeedKmh < 8);
    }

    [Fact]
    public void DetectMode_CyclingSpeed_DetectedAsCycling()
    {
        // 骑车速度约 15 km/h ≈ 4.17 m/s
        // 每秒移动约 0.0000375 度纬度
        var baseTime = DateTime.UtcNow;
        var points = new List<TrackPoint>();
        for (int i = 0; i < 10; i++)
        {
            points.Add(CreatePoint(121.4737, 31.2304 + i * 0.0000375, baseTime.AddSeconds(i)));
        }

        var result = _detector.DetectMode(points);
        Assert.Equal(ActivityType.Cycling, result.Mode);
        Assert.True(result.AverageSpeedKmh >= 8);
        Assert.True(result.AverageSpeedKmh < 25);
    }

    [Fact]
    public void DetectMode_DrivingSpeed_DetectedAsDriving()
    {
        // 驾车速度约 60 km/h ≈ 16.67 m/s
        // 每秒移动约 0.00015 度纬度
        var baseTime = DateTime.UtcNow;
        var points = new List<TrackPoint>();
        for (int i = 0; i < 10; i++)
        {
            points.Add(CreatePoint(121.4737, 31.2304 + i * 0.00015, baseTime.AddSeconds(i)));
        }

        var result = _detector.DetectMode(points);
        Assert.Equal(ActivityType.Driving, result.Mode);
        Assert.True(result.AverageSpeedKmh >= 25);
        Assert.True(result.AverageSpeedKmh < 120);
    }

    [Fact]
    public void DetectMode_TransitWithStops_DetectedAsTransit()
    {
        // 公交：中速 + 频繁停靠
        // 使用连续坐标，避免停靠到行驶的跳变
        var baseTime = DateTime.UtcNow;
        var points = new List<TrackPoint>();
        var currentTime = baseTime;
        var currentLat = 31.2304;

        // 4个行驶段 + 3个停靠段（交替：行驶-停靠-行驶-停靠-行驶-停靠-行驶）
        for (int segment = 0; segment < 7; segment++)
        {
            if (segment % 2 == 0) // 行驶段
            {
                // 行驶20秒，每秒移动约 0.0001 度纬度（约40km/h）
                for (int i = 0; i < 20; i++)
                {
                    points.Add(CreatePoint(121.4737, currentLat, currentTime));
                    currentLat += 0.0001;
                    currentTime = currentTime.AddSeconds(1);
                }
            }
            else // 停靠段
            {
                // 同一位置停留35秒
                for (int i = 0; i < 7; i++)
                {
                    points.Add(CreatePoint(121.4737, currentLat, currentTime));
                    currentTime = currentTime.AddSeconds(5);
                }
            }
        }

        var result = _detector.DetectMode(points);
        Assert.Equal(ActivityType.Transit, result.Mode);
        Assert.True(result.StopCount >= 3);
    }

    [Fact]
    public void DetectMode_FlyingSpeed_DetectedAsFlying()
    {
        // 飞行速度约 800 km/h ≈ 222 m/s
        // 每秒移动约 0.002 度纬度
        var baseTime = DateTime.UtcNow;
        var points = new List<TrackPoint>();
        for (int i = 0; i < 10; i++)
        {
            points.Add(CreatePoint(121.4737, 31.2304 + i * 0.002, baseTime.AddSeconds(i)));
        }

        var result = _detector.DetectMode(points);
        Assert.Equal(ActivityType.Flying, result.Mode);
        Assert.True(result.AverageSpeedKmh >= 200);
    }

    [Fact]
    public void DetectMode_ConfidenceIsBetweenZeroAndOne()
    {
        var baseTime = DateTime.UtcNow;
        var points = new List<TrackPoint>();
        for (int i = 0; i < 10; i++)
        {
            points.Add(CreatePoint(121.4737, 31.2304 + i * 0.0000125, baseTime.AddSeconds(i)));
        }

        var result = _detector.DetectMode(points);
        Assert.True(result.Confidence >= 0);
        Assert.True(result.Confidence <= 1);
    }

    [Fact]
    public void DetectMode_MaxSpeedIsRecorded()
    {
        var baseTime = DateTime.UtcNow;
        var points = new List<TrackPoint>
        {
            CreatePoint(121.4737, 31.2304, baseTime),
            CreatePoint(121.4737, 31.2305, baseTime.AddSeconds(1)), // 快速
            CreatePoint(121.4737, 31.2305001, baseTime.AddSeconds(2)), // 慢速
        };

        var result = _detector.DetectMode(points);
        Assert.True(result.MaxSpeedKmh >= result.AverageSpeedKmh);
    }
}
