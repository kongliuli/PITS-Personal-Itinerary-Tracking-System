using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

public interface ITrackingProfileService
{
    /// <summary>
    /// 获取当前应使用的追踪配置
    /// </summary>
    Task<TrackingProfile> GetCurrentProfileAsync();

    /// <summary>
    /// 根据设备状态选择最佳配置
    /// </summary>
    TrackingProfile SelectProfile(bool isCharging, bool isStationary, bool isDriving);
}
