using Microsoft.EntityFrameworkCore;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Infrastructure.Data;

namespace PITS.MVP.Infrastructure.Services;

public class StatsService : IStatsService
{
    private readonly TripContext _context;

    public StatsService(TripContext context)
    {
        _context = context;
    }

    public async Task<double> GetTotalDistanceAsync()
    {
        var trips = await _context.Trips
            .Include(t => t.TrackPoints)
            .ToListAsync();
        return trips.Sum(t => CalculateTripDistance(t));
    }

    public async Task<int> GetTripCountAsync()
    {
        return await _context.Trips.CountAsync();
    }

    public async Task<int> GetPlaceCountAsync()
    {
        return await _context.Places.CountAsync();
    }

    public async Task<List<PlaceVisit>> GetTopPlacesAsync(int count = 10)
    {
        var places = await _context.Places.ToListAsync();
        var trips = await _context.Trips.Where(t => t.PlaceId != null).ToListAsync();

        return trips.GroupBy(t => t.PlaceId!)
            .Select(g => new PlaceVisit
            {
                PlaceId = g.Key,
                Name = places.FirstOrDefault(p => p.Id == g.Key)?.Name ?? "未知地点",
                VisitCount = g.Count(),
                TotalDurationHours = g.Sum(t => t.EndedAt.HasValue ? (t.EndedAt.Value - t.StartedAt).TotalHours : 0)
            })
            .OrderByDescending(p => p.VisitCount)
            .Take(count)
            .ToList();
    }

    public async Task<Dictionary<int, int>> GetHourDistributionAsync()
    {
        var trips = await _context.Trips.ToListAsync();
        return trips.GroupBy(t => t.StartedAt.Hour)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<DayOfWeek, int>> GetWeekDayDistributionAsync()
    {
        var trips = await _context.Trips.ToListAsync();
        return trips.GroupBy(t => t.StartedAt.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<ActivityType, int>> GetActivityDistributionAsync()
    {
        var trips = await _context.Trips.ToListAsync();
        return trips.GroupBy(t => t.ActivityType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<TimeSpan> GetTotalDurationAsync()
    {
        var trips = await _context.Trips.ToListAsync();
        return TimeSpan.FromTicks(trips
            .Where(t => t.EndedAt.HasValue)
            .Sum(t => (t.EndedAt!.Value - t.StartedAt).Ticks));
    }

    private static double CalculateTripDistance(Trip trip)
    {
        var points = trip.TrackPoints
            .Where(p => p.Location != null)
            .OrderBy(p => p.Timestamp)
            .Select(p => p.Location!)
            .ToList();

        if (points.Count < 2) return 0;

        var meters = 0d;
        for (var i = 1; i < points.Count; i++)
        {
            meters += HaversineMeters(points[i - 1].Y, points[i - 1].X, points[i].Y, points[i].X);
        }

        return meters;
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6371000;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
