using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

public interface IStatsService
{
    Task<double> GetTotalDistanceAsync();
    Task<int> GetTripCountAsync();
    Task<int> GetPlaceCountAsync();
    Task<List<PlaceVisit>> GetTopPlacesAsync(int count = 10);
    Task<Dictionary<int, int>> GetHourDistributionAsync();      // 小时->行程数
    Task<Dictionary<DayOfWeek, int>> GetWeekDayDistributionAsync(); // 星期->行程数
    Task<Dictionary<ActivityType, int>> GetActivityDistributionAsync(); // 活动类型->行程数
    Task<TimeSpan> GetTotalDurationAsync();
}

public class PlaceVisit
{
    public string PlaceId { get; set; } = "";
    public string Name { get; set; } = "";
    public int VisitCount { get; set; }
    public double TotalDurationHours { get; set; }
}
