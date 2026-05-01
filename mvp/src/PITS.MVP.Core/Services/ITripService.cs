using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

public interface ITripService
{
    Task<Trip?> GetByIdAsync(string id);
    Task<IEnumerable<Trip>> GetAllAsync();
    Task<IEnumerable<Trip>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Trip>> GetByActivityTypeAsync(ActivityType activityType);
    Task<IEnumerable<Trip>> GetByVisibilityAsync(VisibilityLevel maxVisibility);
    Task AddAsync(Trip trip);
    Task UpdateAsync(Trip trip);
    Task DeleteAsync(string id);
    Task<IEnumerable<Trip>> SearchAsync(string query, VisibilityLevel maxVisibility);
}
