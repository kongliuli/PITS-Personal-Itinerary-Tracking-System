using Microsoft.EntityFrameworkCore;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Infrastructure.Data;

namespace PITS.MVP.Infrastructure.Services;

public class ReminderService : IReminderService
{
    private readonly TripContext _context;

    public ReminderService(TripContext context)
    {
        _context = context;
    }

    public async Task<List<Trip>> GetOnThisDayTripsAsync(int yearsAgo = 1)
    {
        var targetDate = DateTime.Today.AddYears(-yearsAgo);
        var nextDay = targetDate.AddDays(1);

        return await _context.Trips
            .Where(t => t.StartedAt >= targetDate && t.StartedAt < nextDay)
            .OrderBy(t => t.StartedAt)
            .ToListAsync();
    }

    public async Task<List<OnThisDayResult>> GetAllOnThisDayAsync()
    {
        var results = new List<OnThisDayResult>();

        // 查询过去 5 年
        for (int year = 1; year <= 5; year++)
        {
            var trips = await GetOnThisDayTripsAsync(year);
            if (trips.Any())
            {
                var summary = GenerateSummary(trips, year);
                results.Add(new OnThisDayResult
                {
                    YearsAgo = year,
                    Trips = trips,
                    Summary = summary
                });
            }
        }

        return results;
    }

    private static string GenerateSummary(List<Trip> trips, int yearsAgo)
    {
        var count = trips.Count;
        var firstTrip = trips.First();
        var dateStr = firstTrip.StartedAt.ToString("yyyy年M月d日");

        return $"{yearsAgo}年前的今天（{dateStr}），你有 {count} 条行程记录";
    }
}
