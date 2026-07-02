using Microsoft.EntityFrameworkCore;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Infrastructure.Data;
using NetTopologySuite.Geometries;

namespace PITS.MVP.Infrastructure.Services;

public class PlaceClusterService : IPlaceClusterService
{
    private readonly TripContext _context;
    private readonly IGeocodingService _geocodingService;

    public PlaceClusterService(TripContext context, IGeocodingService geocodingService)
    {
        _context = context;
        _geocodingService = geocodingService;
    }

    public async Task<List<PlaceCluster>> IdentifyPlacesAsync(IReadOnlyList<TrackPoint> points, double clusterRadiusMeters = 50, int minVisitCount = 3)
    {
        // 简单 GeoHash 聚类
        var clusters = new Dictionary<string, PlaceCluster>();

        foreach (var point in points.Where(p => p.Location != null))
        {
            // 使用 6 字符 GeoHash（约 1.2km x 0.6km）
            var hash = PITS.MVP.Core.ValueObjects.GeoHash.Encode(point.Location!.Y, point.Location!.X, 6);

            if (!clusters.ContainsKey(hash))
            {
                clusters[hash] = new PlaceCluster
                {
                    Latitude = point.Location!.Y,
                    Longitude = point.Location!.X,
                    VisitCount = 0,
                    TotalDurationHours = 0
                };
            }

            clusters[hash].VisitCount++;
        }

        // 过滤低频地点
        return clusters.Values
            .Where(c => c.VisitCount >= minVisitCount)
            .OrderByDescending(c => c.VisitCount)
            .ToList();
    }

    public async Task<int> AutoCreatePlacesAsync()
    {
        // 获取所有 TrackPoint，聚类后创建 Place
        var trackPoints = await _context.TrackPoints
            .Where(p => p.Location != null)
            .OrderBy(p => p.Timestamp)
            .ToListAsync();

        var clusters = await IdentifyPlacesAsync(trackPoints);
        var created = 0;

        foreach (var cluster in clusters)
        {
            // 检查是否已存在附近的 Place
            var places = await _context.Places
                .Where(p => p.Location != null)
                .ToListAsync();
            var existingPlace = places.FirstOrDefault(p =>
                DistanceMeters(p.Location!, cluster.Latitude, cluster.Longitude) < 100);

            if (existingPlace == null)
            {
                var place = new Place
                {
                    Id = Ulid.NewUlid().ToString(),
                    Name = cluster.SuggestedName ?? $"地点 {created + 1}",
                    Location = new Point(cluster.Longitude, cluster.Latitude) { SRID = 4326 },
                    GeoHash = PITS.MVP.Core.ValueObjects.GeoHash.Encode(cluster.Latitude, cluster.Longitude, 8),
                    Category = PlaceCategory.Other,
                    VisitCount = cluster.VisitCount
                };

                _context.Places.Add(place);
                created++;
            }
        }

        await _context.SaveChangesAsync();
        return created;
    }

    private static double DistanceMeters(Point point, double latitude, double longitude)
    {
        const double earthRadius = 6371000;
        var lat1 = point.Y * Math.PI / 180;
        var lat2 = latitude * Math.PI / 180;
        var dLat = (latitude - point.Y) * Math.PI / 180;
        var dLon = (longitude - point.X) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
