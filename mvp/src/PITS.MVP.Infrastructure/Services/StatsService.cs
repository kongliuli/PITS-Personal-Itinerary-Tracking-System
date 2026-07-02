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
        // 基于 TrackPoint 计算，或使用 Trip 的 Distance 字段
        var trips = await _context.Trips.ToListAsync();
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
        // 简单估算，实际应基于 TrackPoint
        if (!trip.EndedAt.HasValue || trip.EndedAt.Value == trip.StartedAt) return 0;
        return 0; // 需要实际 TrackPoint 计算
    }
}
