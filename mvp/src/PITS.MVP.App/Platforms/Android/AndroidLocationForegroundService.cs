using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;

namespace PITS.MVP.App;

[Service(Exported = false, ForegroundServiceType = ForegroundService.TypeLocation)]
public class AndroidLocationForegroundService : Service, ILocationListener
{
    private const int NotificationId = 8102;
    private const string ChannelId = "pits_gps_collection";
    private LocationManager? _locationManager;

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        StartForeground(NotificationId, BuildNotification());

        if (!HasLocationPermission())
        {
            AndroidLocationTrackingService.SetStopped("Location permission denied");
            StopSelf();
            return StartCommandResult.NotSticky;
        }

        _locationManager = (LocationManager?)GetSystemService(LocationService);
        if (_locationManager == null)
        {
            AndroidLocationTrackingService.SetStopped("Location service unavailable");
            StopSelf();
            return StartCommandResult.NotSticky;
        }

        var requested = RequestProvider(LocationManager.GpsProvider);
        requested |= RequestProvider(LocationManager.NetworkProvider);

        if (!requested)
        {
            AndroidLocationTrackingService.SetStopped("No enabled location provider");
            StopSelf();
            return StartCommandResult.NotSticky;
        }

        AndroidLocationTrackingService.SetRunning("GPS collection running");
        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        _locationManager?.RemoveUpdates(this);
        AndroidLocationTrackingService.SetStopped("v1 GPS collection stopped");
        base.OnDestroy();
    }

    public void OnLocationChanged(Android.Locations.Location location)
    {
        var service = AndroidLocationTrackingService.Current;
        if (service != null)
            _ = service.SaveLocationAsync(location);
    }

    public void OnProviderDisabled(string provider) { }
    public void OnProviderEnabled(string provider) { }
    public void OnStatusChanged(string? provider, Availability status, Bundle? extras) { }

    private bool RequestProvider(string provider)
    {
        if (_locationManager?.IsProviderEnabled(provider) != true) return false;

        try
        {
            _locationManager.RequestLocationUpdates(
                provider,
                AndroidLocationTrackingService.CurrentOptions.SampleIntervalSeconds * 1000L,
                (float)AndroidLocationTrackingService.CurrentOptions.MinimumDistanceMeters,
                this);
            return true;
        }
        catch (Exception ex)
        {
            AndroidLocationTrackingService.SetStopped($"Location provider failed: {ex.Message}");
            return false;
        }
    }

    private bool HasLocationPermission()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(23)) return true;
        return CheckSelfPermission(global::Android.Manifest.Permission.AccessFineLocation) == Permission.Granted ||
            CheckSelfPermission(global::Android.Manifest.Permission.AccessCoarseLocation) == Permission.Granted;
    }

    private Notification BuildNotification()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var channel = new NotificationChannel(ChannelId, "PITS GPS collection", NotificationImportance.Low);
            ((NotificationManager)GetSystemService(NotificationService)!).CreateNotificationChannel(channel);

            return new Notification.Builder(this, ChannelId)
                .SetContentTitle("PITS GPS collection")
                .SetContentText("Saving location samples")
                .SetSmallIcon(global::Android.Resource.Drawable.IcMenuMyLocation)
                .SetOngoing(true)
                .Build();
        }

        return new Notification.Builder(this)
            .SetContentTitle("PITS GPS collection")
            .SetContentText("Saving location samples")
            .SetSmallIcon(global::Android.Resource.Drawable.IcMenuMyLocation)
            .SetOngoing(true)
            .Build();
    }
}
