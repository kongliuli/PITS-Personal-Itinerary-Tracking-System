using System.Text.Json;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace PITS.MVP.Infrastructure.Services;

public class ImportService : IImportService
{
    private readonly TripContext _context;

    public ImportService(TripContext context)
    {
        _context = context;
    }

    public async Task<ImportResult> ImportFromGoogleTakeoutAsync(Stream jsonStream, IProgress<ImportProgress>? progress = null)
    {
        var result = new ImportResult();

        // Google Takeout JSON 格式: { "locations": [ { "latitudeE7": ..., "longitudeE7": ..., "timestamp": "..." }, ... ] }
        var doc = await JsonDocument.ParseAsync(jsonStream);

        if (!doc.RootElement.TryGetProperty("locations", out var locations))
        {
            result.Errors.Add("无效的 Google Takeout 格式：缺少 locations 数组");
            return result;
        }

        var totalPoints = locations.GetArrayLength();
        var processedPoints = 0;
        var trackPoints = new List<TrackPoint>();

        foreach (var loc in locations.EnumerateArray())
        {
            try
            {
                double latitude = 0, longitude = 0;
                DateTime timestamp = DateTime.MinValue;

                if (loc.TryGetProperty("latitudeE7", out var latE7))
                    latitude = latE7.GetInt64() / 1e7;
                else if (loc.TryGetProperty("latitude", out var lat))
                    latitude = lat.GetDouble();

                if (loc.TryGetProperty("longitudeE7", out var lonE7))
                    longitude = lonE7.GetInt64() / 1e7;
                else if (loc.TryGetProperty("longitude", out var lon))
                    longitude = lon.GetDouble();

                if (loc.TryGetProperty("timestamp", out var ts))
                    timestamp = DateTime.Parse(ts.GetString()!);
                else if (loc.TryGetProperty("timestampMs", out var tsMs))
                    timestamp = DateTimeOffset.FromUnixTimeMilliseconds(tsMs.GetInt64()).DateTime;

                if ((latitude == 0 && longitude == 0) || timestamp == DateTime.MinValue)
                {
                    result.PointsSkipped++;
                    continue;
                }

                var point = new TrackPoint
                {
                    Location = new Point(longitude, latitude) { SRID = 4326 },
                    Timestamp = timestamp,
                    Accuracy = loc.TryGetProperty("accuracy", out var acc) ? acc.GetDouble() : null,
                    TripId = null // 稍后关联
                };

                trackPoints.Add(point);
                result.PointsImported++;
            }
            catch
            {
                result.PointsSkipped++;
            }

            processedPoints++;
            progress?.Report(new ImportProgress
            {
                TotalPoints = totalPoints,
                ProcessedPoints = processedPoints
            });
        }

        // 按时间分组创建 Trip（间隔超过 30 分钟的视为不同行程）
        if (trackPoints.Count > 0)
        {
            var sorted = trackPoints.OrderBy(p => p.Timestamp).ToList();
            var currentTripPoints = new List<TrackPoint> { sorted[0] };

            for (int i = 1; i < sorted.Count; i++)
            {
                var gap = sorted[i].Timestamp - sorted[i - 1].Timestamp;
                if (gap > TimeSpan.FromMinutes(30))
                {
                    // 创建当前行程
                    await CreateTripFromPointsAsync(currentTripPoints);
                    result.TripsCreated++;
                    currentTripPoints = new List<TrackPoint>();
                }
                currentTripPoints.Add(sorted[i]);
            }

            // 创建最后一个行程
            if (currentTripPoints.Count > 0)
            {
                await CreateTripFromPointsAsync(currentTripPoints);
                result.TripsCreated++;
            }
        }

        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<ImportResult> ImportFromGpxAsync(Stream gpxStream, IProgress<ImportProgress>? progress = null)
    {
        var result = new ImportResult();
        var trackPoints = new List<TrackPoint>();

        // 简单 GPX 解析 (使用 XmlReader)
        using var reader = System.Xml.XmlReader.Create(gpxStream);
        double lat = 0, lon = 0;
        DateTime time = DateTime.MinValue;
        bool inTrkpt = false;

        while (reader.Read())
        {
            if (reader.NodeType == System.Xml.XmlNodeType.Element)
            {
                if (reader.Name == "trkpt" || reader.Name == "wpt")
                {
                    inTrkpt = true;
                    lat = double.Parse(reader.GetAttribute("lat") ?? "0");
                    lon = double.Parse(reader.GetAttribute("lon") ?? "0");
                    time = DateTime.MinValue;
                }
                else if (inTrkpt && reader.Name == "time")
                {
                    var timeStr = reader.ReadElementContentAsString();
                    if (DateTime.TryParse(timeStr, out var t))
                        time = t;
                }
            }
            else if (reader.NodeType == System.Xml.XmlNodeType.EndElement)
            {
                if (reader.Name == "trkpt" || reader.Name == "wpt")
                {
                    inTrkpt = false;
                    if (lat != 0 || lon != 0)
                    {
                        trackPoints.Add(new TrackPoint
                        {
                            Location = new Point(lon, lat) { SRID = 4326 },
                            Timestamp = time == DateTime.MinValue ? DateTime.UtcNow : time,
                            TripId = null
                        });
                        result.PointsImported++;
                    }
                    else
                    {
                        result.PointsSkipped++;
                    }
                }
            }
        }

        // 按时间分组创建 Trip
        if (trackPoints.Count > 0)
        {
            var sorted = trackPoints.OrderBy(p => p.Timestamp).ToList();
            var currentTripPoints = new List<TrackPoint> { sorted[0] };

            for (int i = 1; i < sorted.Count; i++)
            {
                var gap = sorted[i].Timestamp - sorted[i - 1].Timestamp;
                if (gap > TimeSpan.FromMinutes(30))
                {
                    await CreateTripFromPointsAsync(currentTripPoints);
                    result.TripsCreated++;
                    currentTripPoints = new List<TrackPoint>();
                }
                currentTripPoints.Add(sorted[i]);
            }

            if (currentTripPoints.Count > 0)
            {
                await CreateTripFromPointsAsync(currentTripPoints);
                result.TripsCreated++;
            }
        }

        await _context.SaveChangesAsync();
        return result;
    }

    private Task CreateTripFromPointsAsync(List<TrackPoint> points)
    {
        if (points.Count == 0) return Task.CompletedTask;

        var trip = new Trip
        {
            Id = Ulid.NewUlid().ToString(),
            StartedAt = points.Min(p => p.Timestamp),
            EndedAt = points.Max(p => p.Timestamp),
            Location = points.FirstOrDefault(p => p.Location != null)?.Location,
            ActivityType = ActivityType.Other,
            Visibility = VisibilityLevel.Private,
            Source = DataSource.Import,
            GeoHash = "" // 稍后计算
        };

        foreach (var point in points)
            point.TripId = trip.Id;

        _context.Trips.Add(trip);
        _context.TrackPoints.AddRange(points);

        return Task.CompletedTask;
    }
}
