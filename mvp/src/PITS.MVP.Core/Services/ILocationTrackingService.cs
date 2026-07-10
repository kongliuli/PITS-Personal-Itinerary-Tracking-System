namespace PITS.MVP.Core.Services;

public interface ILocationTrackingService
{
    Task<LocationTrackingStatus> GetStatusAsync();
    Task StartAsync(LocationTrackingOptions options);
    Task StopAsync();
}

public class LocationSample
{
    public DateTime Timestamp { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Accuracy { get; set; }
    public double? Speed { get; set; }
}

public class LocationTrackingStatus
{
    public bool IsRunning { get; set; }
    public string Message { get; set; } = "";
    public int SavedPointCount { get; set; }
    public DateTime? LastSampleAt { get; set; }
}
