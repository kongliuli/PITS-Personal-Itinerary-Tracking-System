using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

public interface IReminderService
{
    /// <summary>
    /// 获取"去年今日"的行程
    /// </summary>
    Task<List<Trip>> GetOnThisDayTripsAsync(int yearsAgo = 1);

    /// <summary>
    /// 获取所有"今日"的历史行程
    /// </summary>
    Task<List<OnThisDayResult>> GetAllOnThisDayAsync();
}

public class OnThisDayResult
{
    public int YearsAgo { get; set; }
    public List<Trip> Trips { get; set; } = new();
    public string Summary { get; set; } = "";
}
