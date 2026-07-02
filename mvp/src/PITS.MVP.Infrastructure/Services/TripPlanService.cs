using Microsoft.EntityFrameworkCore;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Infrastructure.Data;

namespace PITS.MVP.Infrastructure.Services;

public class TripPlanService : ITripPlanService
{
    private readonly TripContext _context;

    public TripPlanService(TripContext context)
    {
        _context = context;
    }

    public async Task<TripPlan> AddAsync(TripPlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        _context.TripPlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<IReadOnlyList<TripPlan>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.TripPlans
            .Where(p => p.StartsAt >= start && p.StartsAt <= end)
            .OrderBy(p => p.StartsAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TripPlan>> GetUpcomingAsync(DateTime now, int count = 5)
    {
        return await _context.TripPlans
            .Where(p => p.StartsAt >= now && p.Status != PlanStatus.Cancelled)
            .OrderBy(p => p.StartsAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Trip?> ConvertToTripAsync(string planId, DateTime? actualStart = null, DateTime? actualEnd = null)
    {
        var plan = await _context.TripPlans.FindAsync(planId);
        if (plan == null) return null;

        var startedAt = actualStart ?? plan.StartsAt;
        var trip = new Trip
        {
            PlanId = plan.Id,
            StartedAt = startedAt,
            EndedAt = actualEnd ?? plan.EndsAt,
            Timezone = plan.Timezone,
            Location = plan.Location,
            GeoHash = plan.GeoHash,
            Address = plan.LocationName,
            ActivityType = plan.ActivityType,
            Description = plan.Title,
            Visibility = plan.Visibility,
            Source = plan.Source == DataSource.Manual ? DataSource.Manual : DataSource.Import
        };

        plan.Status = PlanStatus.Completed;
        plan.UpdatedAt = DateTime.UtcNow;
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
        return trip;
    }

    public async Task MarkCompletedAsync(string planId)
    {
        var plan = await _context.TripPlans.FindAsync(planId);
        if (plan == null) return;

        plan.Status = PlanStatus.Completed;
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<PlanStats> GetStatsAsync(DateTime start, DateTime end)
    {
        var plans = await _context.TripPlans
            .Include(p => p.ActualTrips)
            .Where(p => p.StartsAt >= start && p.StartsAt <= end)
            .ToListAsync();

        var delays = plans
            .Select(p => new
            {
                Plan = p,
                Actual = p.ActualTrips.OrderBy(t => t.StartedAt).FirstOrDefault()
            })
            .Where(x => x.Actual != null)
            .Select(x => Math.Max(0, (x.Actual!.StartedAt - x.Plan.StartsAt).TotalMinutes))
            .ToList();

        return new PlanStats
        {
            PlannedCount = plans.Count,
            CompletedCount = plans.Count(p => p.Status == PlanStatus.Completed || p.ActualTrips.Count > 0),
            DelayedCount = plans.Count(p => p.ActualTrips.Any(t => t.StartedAt > p.StartsAt.AddMinutes(15))),
            AverageDelayMinutes = delays.Count == 0 ? 0 : delays.Average()
        };
    }
}
