using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class TrackingProfileService : ITrackingProfileService
{
    // 预定义配置文件
    private static readonly List<TrackingProfile> DefaultProfiles = new()
    {
        new TrackingProfile
        {
            Name = "高频追踪",
            UpdateIntervalSeconds = 5,
            MinimumDistanceMeters = 5,
            Trigger = ProfileTrigger.Charging,
            IsEnabled = true
        },
        new TrackingProfile
        {
            Name = "标准追踪",
            UpdateIntervalSeconds = 30,
            MinimumDistanceMeters = 10,
            Trigger = ProfileTrigger.Always,
            IsEnabled = true
        },
        new TrackingProfile
        {
            Name = "低频追踪",
            UpdateIntervalSeconds = 300,
            MinimumDistanceMeters = 100,
            Trigger = ProfileTrigger.Stationary,
            IsEnabled = true
        },
        new TrackingProfile
        {
            Name = "驾驶追踪",
            UpdateIntervalSeconds = 10,
            MinimumDistanceMeters = 20,
            Trigger = ProfileTrigger.Driving,
            IsEnabled = true
        }
    };

    public Task<TrackingProfile> GetCurrentProfileAsync()
    {
        // 返回默认标准配置
        return Task.FromResult(DefaultProfiles.First(p => p.Trigger == ProfileTrigger.Always));
    }

    public TrackingProfile SelectProfile(bool isCharging, bool isStationary, bool isDriving)
    {
        // 优先级：驾驶 > 充电 > 静止 > 标准
        if (isDriving)
            return DefaultProfiles.First(p => p.Trigger == ProfileTrigger.Driving);
        if (isCharging)
            return DefaultProfiles.First(p => p.Trigger == ProfileTrigger.Charging);
        if (isStationary)
            return DefaultProfiles.First(p => p.Trigger == ProfileTrigger.Stationary);
        return DefaultProfiles.First(p => p.Trigger == ProfileTrigger.Always);
    }
}
