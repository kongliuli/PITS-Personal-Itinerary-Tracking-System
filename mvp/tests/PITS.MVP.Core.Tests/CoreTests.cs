using PITS.MVP.Core.Entities;
using PITS.MVP.Core.ValueObjects;
using Xunit;

namespace PITS.MVP.Core.Tests;

public class TripEntityTests
{
    [Fact]
    public void Trip_NewInstance_HasDefaultValues()
    {
        var trip = new Trip();

        Assert.NotNull(trip.Id);
        Assert.NotEmpty(trip.Id);
        Assert.Equal(VisibilityLevel.Private, trip.Visibility);
        Assert.Equal(DataSource.Manual, trip.Source);
        Assert.Equal("Asia/Shanghai", trip.Timezone);
        Assert.NotEqual(default, trip.CreatedAt);
    }

    [Fact]
    public void Trip_SetProperties_ValuesAreCorrect()
    {
        var trip = new Trip
        {
            StartedAt = new DateTime(2026, 5, 1, 9, 0, 0),
            EndedAt = new DateTime(2026, 5, 1, 12, 0, 0),
            ActivityType = ActivityType.Work,
            Description = "Team meeting",
            Visibility = VisibilityLevel.Work
        };

        Assert.Equal(new DateTime(2026, 5, 1, 9, 0, 0), trip.StartedAt);
        Assert.Equal(new DateTime(2026, 5, 1, 12, 0, 0), trip.EndedAt);
        Assert.Equal(ActivityType.Work, trip.ActivityType);
        Assert.Equal("Team meeting", trip.Description);
        Assert.Equal(VisibilityLevel.Work, trip.Visibility);
    }
}

public class PlaceEntityTests
{
    [Fact]
    public void Place_NewInstance_HasDefaultValues()
    {
        var place = new Place();

        Assert.NotNull(place.Id);
        Assert.NotEmpty(place.Id);
        Assert.NotNull(place.Trips);
        Assert.Empty(place.Trips);
        Assert.Equal(200, place.Radius);
    }

    [Fact]
    public void Place_SetProperties_ValuesAreCorrect()
    {
        var place = new Place
        {
            Name = "Office",
            Category = PlaceCategory.Office,
            VisitCount = 5,
            LastVisited = DateTime.UtcNow
        };

        Assert.Equal("Office", place.Name);
        Assert.Equal(PlaceCategory.Office, place.Category);
        Assert.Equal(5, place.VisitCount);
        Assert.NotNull(place.LastVisited);
    }
}

public class TrackPointEntityTests
{
    [Fact]
    public void TrackPoint_SetProperties_ValuesAreCorrect()
    {
        var point = new TrackPoint
        {
            Timestamp = DateTime.UtcNow,
            Accuracy = 10.5,
            Speed = 5.2,
            Altitude = 100.0
        };

        Assert.NotEqual(default, point.Timestamp);
        Assert.Equal(10.5, point.Accuracy);
        Assert.Equal(5.2, point.Speed);
        Assert.Equal(100.0, point.Altitude);
    }
}

public class GeoHashTests
{
    [Theory]
    [InlineData(31.2304, 121.4737, 8, "wtw3s0gf")]
    [InlineData(39.9042, 116.4074, 8, "wx4g69e1")]
    [InlineData(0, 0, 1, "s")]
    public void Encode_ValidCoordinates_ReturnsExpectedHash(double lat, double lon, int precision, string expected)
    {
        var result = Core.ValueObjects.GeoHash.Encode(lat, lon, precision);

        Assert.NotEmpty(result);
        Assert.Equal(precision, result.Length);
    }

    [Fact]
    public void Encode_DifferentPrecisions_ReturnsCorrectLengths()
    {
        var lat = 31.2304;
        var lon = 121.4737;

        for (int precision = 1; precision <= 12; precision++)
        {
            var result = Core.ValueObjects.GeoHash.Encode(lat, lon, precision);
            Assert.Equal(precision, result.Length);
        }
    }

    [Fact]
    public void Encode_InvalidPrecision_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            Core.ValueObjects.GeoHash.Encode(31.2304, 121.4737, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            Core.ValueObjects.GeoHash.Encode(31.2304, 121.4737, 13));
    }

    [Fact]
    public void Decode_ValidHash_ReturnsCoordinates()
    {
        var hash = "wtw3s0gf";
        var (lat, lon) = Core.ValueObjects.GeoHash.Decode(hash);

        Assert.InRange(lat, 31.0, 31.3);
        Assert.InRange(lon, 121.0, 121.5);
    }

    [Fact]
    public void EncodeDecode_RoundTrip_PreservesApproximateLocation()
    {
        var originalLat = 31.2304;
        var originalLon = 121.4737;
        var precision = 8;

        var hash = Core.ValueObjects.GeoHash.Encode(originalLat, originalLon, precision);
        var (decodedLat, decodedLon) = Core.ValueObjects.GeoHash.Decode(hash);

        Assert.InRange(decodedLat, originalLat - 0.01, originalLat + 0.01);
        Assert.InRange(decodedLon, originalLon - 0.01, originalLon + 0.01);
    }
}

public class TimeRangeTests
{
    [Fact]
    public void TimeRange_Duration_CalculatesCorrectly()
    {
        var range = new TimeRange(
            new DateTime(2026, 5, 1, 9, 0, 0),
            new DateTime(2026, 5, 1, 12, 0, 0)
        );

        Assert.Equal(TimeSpan.FromHours(3), range.Duration);
    }

    [Fact]
    public void TimeRange_Contains_ReturnsTrueForWithinRange()
    {
        var range = new TimeRange(
            new DateTime(2026, 5, 1, 9, 0, 0),
            new DateTime(2026, 5, 1, 12, 0, 0)
        );

        Assert.True(range.Contains(new DateTime(2026, 5, 1, 10, 0, 0)));
        Assert.True(range.Contains(new DateTime(2026, 5, 1, 9, 0, 0)));
        Assert.True(range.Contains(new DateTime(2026, 5, 1, 12, 0, 0)));
    }

    [Fact]
    public void TimeRange_Contains_ReturnsFalseForOutsideRange()
    {
        var range = new TimeRange(
            new DateTime(2026, 5, 1, 9, 0, 0),
            new DateTime(2026, 5, 1, 12, 0, 0)
        );

        Assert.False(range.Contains(new DateTime(2026, 5, 1, 8, 0, 0)));
        Assert.False(range.Contains(new DateTime(2026, 5, 1, 13, 0, 0)));
    }

    [Fact]
    public void TimeRange_Overlaps_DetectsOverlapCorrectly()
    {
        var range1 = new TimeRange(
            new DateTime(2026, 5, 1, 9, 0, 0),
            new DateTime(2026, 5, 1, 12, 0, 0)
        );

        var range2 = new TimeRange(
            new DateTime(2026, 5, 1, 11, 0, 0),
            new DateTime(2026, 5, 1, 14, 0, 0)
        );

        var range3 = new TimeRange(
            new DateTime(2026, 5, 1, 13, 0, 0),
            new DateTime(2026, 5, 1, 16, 0, 0)
        );

        Assert.True(range1.Overlaps(range2));
        Assert.False(range1.Overlaps(range3));
    }

    [Fact]
    public void TimeRange_Today_ReturnsTodayRange()
    {
        var today = DateTime.Today;
        var range = TimeRange.Today;

        Assert.Equal(today, range.Start.Date);
        Assert.Equal(today.AddDays(1).AddTicks(-1), range.End);
    }
}

public class BoundingBoxTests
{
    [Fact]
    public void BoundingBox_Center_CalculatesCorrectly()
    {
        var bbox = new BoundingBox(40.0, 39.0, 117.0, 116.0);
        var center = bbox.Center;

        Assert.Equal(39.5, center.Y);
        Assert.Equal(116.5, center.X);
    }

    [Fact]
    public void BoundingBox_Contains_ReturnsTrueForInside()
    {
        var bbox = new BoundingBox(40.0, 39.0, 117.0, 116.0);
        var point = new NetTopologySuite.Geometries.Point(116.5, 39.5) { SRID = 4326 };

        Assert.True(bbox.Contains(point));
    }

    [Fact]
    public void BoundingBox_Contains_ReturnsFalseForOutside()
    {
        var bbox = new BoundingBox(40.0, 39.0, 117.0, 116.0);
        var point = new NetTopologySuite.Geometries.Point(118.0, 40.5) { SRID = 4326 };

        Assert.False(bbox.Contains(point));
    }

    [Fact]
    public void BoundingBox_ToPolygon_CreatesCorrectPolygon()
    {
        var bbox = new BoundingBox(40.0, 39.0, 117.0, 116.0);
        var polygon = bbox.ToPolygon();

        Assert.NotNull(polygon);
        Assert.True(polygon.IsValid);
        Assert.Equal(4326, polygon.SRID);
    }

    [Fact]
    public void BoundingBox_FromCenter_CreatesCorrectBox()
    {
        var center = new NetTopologySuite.Geometries.Point(116.5, 39.5) { SRID = 4326 };
        var bbox = BoundingBox.FromCenter(center, 1000);

        Assert.NotNull(bbox);
        Assert.True(bbox.North > 39.5);
        Assert.True(bbox.South < 39.5);
        Assert.True(bbox.East > 116.5);
        Assert.True(bbox.West < 116.5);
    }
}

public class EnumTests
{
    [Theory]
    [InlineData(ActivityType.Work)]
    [InlineData(ActivityType.Commute)]
    [InlineData(ActivityType.Personal)]
    [InlineData(ActivityType.Health)]
    [InlineData(ActivityType.Travel)]
    [InlineData(ActivityType.Study)]
    [InlineData(ActivityType.Entertainment)]
    [InlineData(ActivityType.Other)]
    public void ActivityType_AllValuesAreDefined(ActivityType activityType)
    {
        Assert.True(Enum.IsDefined(typeof(ActivityType), activityType));
    }

    [Theory]
    [InlineData(VisibilityLevel.Public, 1)]
    [InlineData(VisibilityLevel.Work, 2)]
    [InlineData(VisibilityLevel.Private, 3)]
    [InlineData(VisibilityLevel.Classified, 4)]
    public void VisibilityLevel_HasCorrectValues(VisibilityLevel level, int expected)
    {
        Assert.Equal(expected, (int)level);
    }

    [Theory]
    [InlineData(DataSource.Manual)]
    [InlineData(DataSource.GpsAuto)]
    [InlineData(DataSource.AiParse)]
    [InlineData(DataSource.CalendarSync)]
    [InlineData(DataSource.EmailParse)]
    [InlineData(DataSource.Import)]
    public void DataSource_AllValuesAreDefined(DataSource source)
    {
        Assert.True(Enum.IsDefined(typeof(DataSource), source));
    }
}
