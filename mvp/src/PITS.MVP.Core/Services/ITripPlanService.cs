using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

public interface ITripPlanService
{
    Task<TripPlan> AddAsync(TripPlan plan);
    Task<IReadOnlyList<TripPlan>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IReadOnlyList<TripPlan>> GetUpcomingAsync(DateTime now, int count = 5);
    Task<Trip?> ConvertToTripAsync(string planId, DateTime? actualStart = null, DateTime? actualEnd = null);
    Task MarkCompletedAsync(string planId);
    Task<PlanStats> GetStatsAsync(DateTime start, DateTime end);
}

public class PlanStats
{
    public int PlannedCount { get; set; }
    public int CompletedCount { get; set; }
    public int DelayedCount { get; set; }
    public double CompletionRate => PlannedCount == 0 ? 0 : (double)CompletedCount / PlannedCount;
}
