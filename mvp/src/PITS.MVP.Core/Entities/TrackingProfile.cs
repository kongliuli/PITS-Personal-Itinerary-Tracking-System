namespace PITS.MVP.Core.Entities;

public class TrackingProfile
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string Name { get; set; } = "";

    /// <summary>
    /// GPS 更新间隔（秒）
    /// </summary>
    public int UpdateIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// 最小距离过滤器（米）
    /// </summary>
    public double MinimumDistanceMeters { get; set; } = 10;

    /// <summary>
    /// 触发条件
    /// </summary>
    public ProfileTrigger Trigger { get; set; } = ProfileTrigger.Always;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

public enum ProfileTrigger
{
    Always,         // 始终
    Charging,       // 充电时
    NotCharging,    // 未充电时
    Stationary,     // 静止时
    Moving,         // 移动时
    Driving         // 驾驶时
}
