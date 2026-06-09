using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Core.ValueObjects;
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

public class TripSegmentAnalyzerTests
{
    private readonly Mock<ITransportModeDetector> _modeDetectorMock;
    private readonly TripSegmentAnalyzer _analyzer;

    public TripSegmentAnalyzerTests()
    {
        _modeDetectorMock = new Mock<ITransportModeDetector>();
        _modeDetectorMock
            .Setup(d => d.DetectMode(It.IsAny<IReadOnlyList<TrackPoint>>()))
            .Returns(new TransportModeResult { Mode = ActivityType.Commute, Confidence = 0.8 });

        _analyzer = new TripSegmentAnalyzer(_modeDetectorMock.Object);
    }

    [Fact]
    public void Analyze_EmptyPoints_ReturnsEmptyList()
    {
        var result = _analyzer.Analyze(new List<TrackPoint>());
        Assert.Empty(result);
    }

    [Fact]
    public void Analyze_NullPoints_ReturnsEmptyList()
    {
        var result = _analyzer.Analyze(null!);
        Assert.Empty(result);
    }

    [Fact]
    public void Analyze_StayPoints_ReturnsStaySegment()
    {
        // 创建一组在 50 米半径内停留超过 5 分钟的点
        var baseTime = DateTime.UtcNow;
        var basePoint = new Point(121.4737, 31.2304) { SRID = 4326 };

        var points = new List<TrackPoint>();
        for (int i = 0; i <= 10; i++)
        {
            points.Add(new TrackPoint
            {
                Timestamp = baseTime.AddMinutes(i),
                Location = new Point(121.4737 + i * 0.00001, 31.2304) { SRID = 4326 } // 约 1 米偏移
            });
        }

        var result = _analyzer.Analyze(points);

        Assert.NotEmpty(result);
        Assert.Contains(result, s => s.Type == SegmentType.Stay);
    }

    [Fact]
    public void Analyze_MovingPoints_ReturnsTripSegment()
    {
        // 创建一组快速移动的点（模拟驾车）
        var baseTime = DateTime.UtcNow;

        var points = new List<TrackPoint>
        {
            new TrackPoint { Timestamp = baseTime, Location = new Point(121.4737, 31.2304) { SRID = 4326 } },
            new TrackPoint { Timestamp = baseTime.AddMinutes(1), Location = new Point(121.4837, 31.2404) { SRID = 4326 } }, // ~1.5km
            new TrackPoint { Timestamp = baseTime.AddMinutes(2), Location = new Point(121.4937, 31.2504) { SRID = 4326 } }, // ~1.5km
        };

        var result = _analyzer.Analyze(points);

        Assert.NotEmpty(result);
        Assert.Contains(result, s => s.Type == SegmentType.Trip);
    }

    [Fact]
    public void Analyze_GapBetweenPoints_ReturnsGapSegment()
    {
        // 创建两组时间间隔超过 30 分钟的点
        var baseTime = DateTime.UtcNow;

        var points = new List<TrackPoint>
        {
            new TrackPoint { Timestamp = baseTime, Location = new Point(121.4737, 31.2304) { SRID = 4326 } },
            new TrackPoint { Timestamp = baseTime.AddMinutes(1), Location = new Point(121.4837, 31.2404) { SRID = 4326 } },
            new TrackPoint { Timestamp = baseTime.AddMinutes(60), Location = new Point(121.4937, 31.2504) { SRID = 4326 } },
            new TrackPoint { Timestamp = baseTime.AddMinutes(61), Location = new Point(121.5037, 31.2604) { SRID = 4326 } },
        };

        var result = _analyzer.Analyze(points);

        Assert.NotEmpty(result);
        Assert.Contains(result, s => s.Type == SegmentType.Gap);
    }

    [Fact]
    public void Analyze_StayThenTrip_ReturnsBothSegments()
    {
        // 先停留，然后出行
        var baseTime = DateTime.UtcNow;

        var points = new List<TrackPoint>();

        // 停留 10 分钟（小范围移动）
        for (int i = 0; i <= 10; i++)
        {
            points.Add(new TrackPoint
            {
                Timestamp = baseTime.AddMinutes(i),
                Location = new Point(121.4737 + i * 0.00001, 31.2304) { SRID = 4326 }
            });
        }

        // 出行（大范围移动）
        for (int i = 1; i <= 3; i++)
        {
            points.Add(new TrackPoint
            {
                Timestamp = baseTime.AddMinutes(10 + i),
                Location = new Point(121.4737 + i * 0.01, 31.2304) { SRID = 4326 } // 每分钟约 1km
            });
        }

        var result = _analyzer.Analyze(points);

        Assert.NotEmpty(result);
        Assert.Contains(result, s => s.Type == SegmentType.Stay);
        Assert.Contains(result, s => s.Type == SegmentType.Trip);
    }

    [Fact]
    public void Analyze_TripSegment_CallsModeDetector()
    {
        var baseTime = DateTime.UtcNow;

        var points = new List<TrackPoint>
        {
            new TrackPoint { Timestamp = baseTime, Location = new Point(121.4737, 31.2304) { SRID = 4326 } },
            new TrackPoint { Timestamp = baseTime.AddMinutes(1), Location = new Point(121.4837, 31.2404) { SRID = 4326 } },
            new TrackPoint { Timestamp = baseTime.AddMinutes(2), Location = new Point(121.4937, 31.2504) { SRID = 4326 } },
        };

        var result = _analyzer.Analyze(points);

        _modeDetectorMock.Verify(d => d.DetectMode(It.IsAny<IReadOnlyList<TrackPoint>>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Analyze_TripSegment_HasDistance()
    {
        var baseTime = DateTime.UtcNow;

        var points = new List<TrackPoint>
        {
            new TrackPoint { Timestamp = baseTime, Location = new Point(121.4737, 31.2304) { SRID = 4326 } },
            new TrackPoint { Timestamp = baseTime.AddMinutes(1), Location = new Point(121.4837, 31.2404) { SRID = 4326 } },
            new TrackPoint { Timestamp = baseTime.AddMinutes(2), Location = new Point(121.4937, 31.2504) { SRID = 4326 } },
        };

        var result = _analyzer.Analyze(points);
        var tripSegment = result.FirstOrDefault(s => s.Type == SegmentType.Trip);

        Assert.NotNull(tripSegment);
        Assert.True(tripSegment.DistanceMeters > 0);
    }

    [Fact]
    public void Analyze_StaySegment_HasCenterLocation()
    {
        var baseTime = DateTime.UtcNow;

        var points = new List<TrackPoint>();
        for (int i = 0; i <= 10; i++)
        {
            points.Add(new TrackPoint
            {
                Timestamp = baseTime.AddMinutes(i),
                Location = new Point(121.4737 + i * 0.00001, 31.2304) { SRID = 4326 }
            });
        }

        var result = _analyzer.Analyze(points);
        var staySegment = result.FirstOrDefault(s => s.Type == SegmentType.Stay);

        Assert.NotNull(staySegment);
        Assert.NotNull(staySegment.CenterLocation);
    }
}
