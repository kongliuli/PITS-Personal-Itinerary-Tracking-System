using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class LocationTrackingService : ILocationTrackingService
{
    public Task<LocationTrackingStatus> GetStatusAsync()
    {
        return Task.FromResult(new LocationTrackingStatus
        {
            IsRunning = false,
            Message = "v1 后台定位未启动"
        });
    }

    public Task StartAsync(LocationTrackingOptions options)
    {
        throw new NotSupportedException("v1 后台定位平台实现尚未接入。");
    }

    public Task StopAsync() => Task.CompletedTask;
}
