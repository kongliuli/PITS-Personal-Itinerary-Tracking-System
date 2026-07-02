using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Infrastructure.Data;
using PITS.MVP.Infrastructure.Services;
using System.Text;
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

public class TripContextSchemaTests
{
    [Fact]
    public async Task EnsureReady_AddsPlanTablesToExistingDatabase()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "pits-schema-test-" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        var dbPath = Path.Combine(tempDir, "pits.db");

        try
        {
            await using var context = CreateFileContext(dbPath);
            await context.Database.ExecuteSqlRawAsync("""
CREATE TABLE Trips (
    Id TEXT NOT NULL PRIMARY KEY,
    StartedAt TEXT NOT NULL,
    ActivityType TEXT NOT NULL,
    Visibility TEXT NOT NULL
);
""");

            TripContextSchema.EnsureReady(context);

            context.TripPlans.Add(new TripPlan
            {
                Title = "schema smoke",
                StartsAt = new DateTime(2026, 7, 7, 9, 0, 0)
            });
            await context.SaveChangesAsync();

            Assert.Equal(1, await context.TripPlans.CountAsync());
        }
        finally
        {
            if (tempDir.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    private static TripContext CreateFileContext(string dbPath)
    {
        var options = new DbContextOptionsBuilder<TripContext>()
            .UseSqlite($"Data Source={dbPath};Pooling=False", sqliteOptions => sqliteOptions.UseNetTopologySuite())
            .Options;

        return new TripContext(options);
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

public class TripPlanServiceTests : IDisposable
{
    private readonly TripContext _context;
    private readonly TripPlanService _service;

    public TripPlanServiceTests()
    {
        var options = new DbContextOptionsBuilder<TripContext>()
            .UseSqlite("DataSource=:memory:", sqliteOptions => sqliteOptions.UseNetTopologySuite())
            .Options;

        _context = new TripContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _service = new TripPlanService(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public async Task ConvertToTrip_MarksPlanCompletedAndLinksTrip()
    {
        var plan = await _service.AddAsync(new TripPlan
        {
            Title = "客户会议",
            StartsAt = new DateTime(2026, 7, 3, 9, 0, 0),
            EndsAt = new DateTime(2026, 7, 3, 10, 0, 0),
            ActivityType = ActivityType.Work
        });

        var trip = await _service.ConvertToTripAsync(plan.Id, plan.StartsAt.AddMinutes(20), plan.EndsAt);

        Assert.NotNull(trip);
        Assert.Equal(plan.Id, trip.PlanId);
        Assert.Equal(PlanStatus.Completed, (await _context.TripPlans.FindAsync(plan.Id))!.Status);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsPlanActualClosure()
    {
        var onTime = await _service.AddAsync(new TripPlan
        {
            Title = "准时会议",
            StartsAt = new DateTime(2026, 7, 6, 9, 0, 0),
            EndsAt = new DateTime(2026, 7, 6, 10, 0, 0)
        });
        var delayed = await _service.AddAsync(new TripPlan
        {
            Title = "延误会议",
            StartsAt = new DateTime(2026, 7, 6, 14, 0, 0),
            EndsAt = new DateTime(2026, 7, 6, 15, 0, 0)
        });

        await _service.ConvertToTripAsync(onTime.Id, onTime.StartsAt, onTime.EndsAt);
        await _service.ConvertToTripAsync(delayed.Id, delayed.StartsAt.AddMinutes(30), delayed.EndsAt);

        var stats = await _service.GetStatsAsync(new DateTime(2026, 7, 1), new DateTime(2026, 8, 1));

        Assert.Equal(2, stats.PlannedCount);
        Assert.Equal(2, stats.CompletedCount);
        Assert.Equal(1, stats.DelayedCount);
        Assert.Equal(15, stats.AverageDelayMinutes);
        Assert.Equal(1, stats.CompletionRate);
    }
}

public class ImportStagingTests : IDisposable
{
    private readonly TripContext _context;
    private readonly ImportService _service;

    public ImportStagingTests()
    {
        var options = new DbContextOptionsBuilder<TripContext>()
            .UseSqlite("DataSource=:memory:", sqliteOptions => sqliteOptions.UseNetTopologySuite())
            .Options;

        _context = new TripContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        _service = new ImportService(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public async Task StageIcs_DeduplicatesByFingerprint()
    {
        var ics = """
BEGIN:VCALENDAR
BEGIN:VEVENT
UID:pits-test-1
DTSTART:20260703T090000
DTEND:20260703T100000
SUMMARY:客户会议
LOCATION:上海
END:VEVENT
END:VCALENDAR
""";

        await _service.StageIcsAsync(new MemoryStream(Encoding.UTF8.GetBytes(ics)));
        var second = await _service.StageIcsAsync(new MemoryStream(Encoding.UTF8.GetBytes(ics)));

        Assert.Equal(0, second.ItemsStaged);
        Assert.Single(await _service.GetPendingStagingItemsAsync());
    }

    [Fact]
    public async Task ConfirmStagingItemAsPlan_CreatesPlannedTripPlan()
    {
        var ics = """
BEGIN:VCALENDAR
BEGIN:VEVENT
UID:pits-test-2
DTSTART:20260704T090000
SUMMARY:出差
END:VEVENT
END:VCALENDAR
""";

        await _service.StageIcsAsync(new MemoryStream(Encoding.UTF8.GetBytes(ics)));
        var item = (await _service.GetPendingStagingItemsAsync()).Single();
        var plan = await _service.ConfirmStagingItemAsPlanAsync(item.Id);

        Assert.NotNull(plan);
        Assert.Equal(PlanStatus.Planned, plan.Status);
        Assert.Equal(DataSource.CalendarSync, plan.Source);
        Assert.Empty(await _service.GetPendingStagingItemsAsync());
    }

    [Fact]
    public async Task StageEmailAsync_StagesConfirmationAsPlan()
    {
        var email = """
Subject: 上海客户会议
日期: 2026-07-05 09:30
地点: 上海虹桥
""";

        var result = await _service.StageEmailAsync(new MemoryStream(Encoding.UTF8.GetBytes(email)));
        var item = (await _service.GetPendingStagingItemsAsync()).Single();

        Assert.Equal(1, result.ItemsStaged);
        Assert.Equal(DataSource.EmailParse, item.Source);
        Assert.Equal("上海客户会议", item.Title);
        Assert.Equal("上海虹桥", item.LocationName);

        var plan = await _service.ConfirmStagingItemAsPlanAsync(item.Id);

        Assert.NotNull(plan);
        Assert.Equal(DataSource.EmailParse, plan.Source);
        Assert.Equal(new DateTime(2026, 7, 5, 9, 30, 0), plan.StartsAt);
        Assert.Equal("上海虹桥", plan.LocationName);
    }
}

public class BackupServiceTests
{
    [Fact]
    public async Task RestoreAsync_ReplacesDatabaseFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "pits-backup-test-" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        var dbPath = Path.Combine(tempDir, "pits.db");

        try
        {
            await using (var context = CreateFileContext(dbPath))
            {
                await context.Database.EnsureCreatedAsync();
                context.Trips.Add(new Trip { StartedAt = DateTime.UtcNow, ActivityType = ActivityType.Work });
                await context.SaveChangesAsync();

                var service = new BackupService(context);
                var backupPath = await service.BackupAsync(tempDir);

                context.Trips.Add(new Trip { StartedAt = DateTime.UtcNow, ActivityType = ActivityType.Personal });
                await context.SaveChangesAsync();

                await service.RestoreAsync(backupPath);
            }

            await using (var restored = CreateFileContext(dbPath))
            {
                Assert.Equal(1, await restored.Trips.CountAsync());
            }
        }
        finally
        {
            if (tempDir.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    private static TripContext CreateFileContext(string dbPath)
    {
        var options = new DbContextOptionsBuilder<TripContext>()
            .UseSqlite($"Data Source={dbPath};Pooling=False", sqliteOptions => sqliteOptions.UseNetTopologySuite())
            .Options;

        return new TripContext(options);
    }
}

public class PrivacyExportServiceTests
{
    [Fact]
    public void RedactTrip_HidesClassifiedDetails()
    {
        var service = new PrivacyExportService();
        var trip = new Trip
        {
            StartedAt = DateTime.UtcNow,
            ActivityType = ActivityType.Work,
            Visibility = VisibilityLevel.Classified,
            Description = "secret",
            Address = "home",
            Location = new Point(121.473701, 31.230401) { SRID = 4326 }
        };

        var row = service.RedactTrip(trip, VisibilityLevel.Private);

        Assert.Null(row.Description);
        Assert.Null(row.Address);
        Assert.Null(row.Latitude);
        Assert.Null(row.Longitude);
    }
}
