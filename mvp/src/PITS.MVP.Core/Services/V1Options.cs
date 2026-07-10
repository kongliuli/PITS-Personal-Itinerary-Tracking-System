namespace PITS.MVP.Core.Services;

public class AlmanacOptions
{
    public string ApiBaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
}

public class MqttOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = "";
    public int Port { get; set; } = 1883;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LocationTrackingOptions
{
    public bool Enabled { get; set; }
    public int SampleIntervalSeconds { get; set; } = 30;
    public double MinimumDistanceMeters { get; set; } = 10;
    public double GeofenceRadiusMeters { get; set; } = 200;
    public double StayRadiusMeters { get; set; } = 50;
    public double StayDurationMinutes { get; set; } = 5;
    public double GapThresholdMinutes { get; set; } = 30;

    public bool ShouldSave(LocationSample? previous, LocationSample current)
    {
        if (previous == null) return true;
        if (current.Timestamp - previous.Timestamp >= TimeSpan.FromSeconds(SampleIntervalSeconds)) return true;
        return DistanceMeters(previous, current) >= MinimumDistanceMeters;
    }

    private static double DistanceMeters(LocationSample a, LocationSample b)
    {
        const double earthRadiusMeters = 6371000;
        var lat1 = ToRadians(a.Latitude);
        var lat2 = ToRadians(b.Latitude);
        var dLat = ToRadians(b.Latitude - a.Latitude);
        var dLon = ToRadians(b.Longitude - a.Longitude);
        var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1) * Math.Cos(lat2) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
