using Android.Content;
using Android.Locations;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.App;

public class AndroidLocationTrackingService : ILocationTrackingService
{
    private readonly ITripService _tripService;
    private readonly SemaphoreSlim _saveGate = new(1, 1);
    private LocationSample? _lastSavedSample;

    internal static AndroidLocationTrackingService? Current { get; private set; }
    internal static LocationTrackingOptions CurrentOptions { get; private set; } = new();
    internal static LocationTrackingStatus CurrentStatus { get; private set; } = new()
    {
        Message = "v1 GPS collection stopped"
    };

    public AndroidLocationTrackingService(ITripService tripService)
    {
        _tripService = tripService;
        Current = this;
    }

    public Task<LocationTrackingStatus> GetStatusAsync() => Task.FromResult(CurrentStatus);

    public Task StartAsync(LocationTrackingOptions options)
    {
        Current = this;
        CurrentOptions = options;
        var intent = new Intent(global::Android.App.Application.Context, typeof(AndroidLocationForegroundService));
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
            global::Android.App.Application.Context.StartForegroundService(intent);
        else
            global::Android.App.Application.Context.StartService(intent);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        var intent = new Intent(global::Android.App.Application.Context, typeof(AndroidLocationForegroundService));
        global::Android.App.Application.Context.StopService(intent);
        SetStopped("v1 GPS collection stopped");
        return Task.CompletedTask;
    }

    internal async Task SaveLocationAsync(Android.Locations.Location location)
    {
        var sample = new LocationSample
        {
            Timestamp = DateTime.UtcNow,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Accuracy = location.HasAccuracy ? location.Accuracy : null,
            Speed = location.HasSpeed ? location.Speed : null
        };

        await _saveGate.WaitAsync();
        try
        {
            if (!CurrentOptions.ShouldSave(_lastSavedSample, sample)) return;

            await _tripService.AddTrackPointAsync(new TrackPoint
            {
                TripId = null,
                Timestamp = sample.Timestamp,
                Location = new NetTopologySuite.Geometries.Point(sample.Longitude, sample.Latitude) { SRID = 4326 },
                Accuracy = sample.Accuracy,
                Speed = sample.Speed,
                Altitude = location.HasAltitude ? location.Altitude : null
            });

            _lastSavedSample = sample;
            CurrentStatus = new LocationTrackingStatus
            {
                IsRunning = true,
                SavedPointCount = CurrentStatus.SavedPointCount + 1,
                LastSampleAt = sample.Timestamp,
                Message = $"GPS collection running, saved {CurrentStatus.SavedPointCount + 1} points"
            };
        }
        finally
        {
            _saveGate.Release();
        }
    }

    internal static void SetRunning(string message)
    {
        CurrentStatus = new LocationTrackingStatus
        {
            IsRunning = true,
            SavedPointCount = CurrentStatus.SavedPointCount,
            LastSampleAt = CurrentStatus.LastSampleAt,
            Message = message
        };
    }

    internal static void SetStopped(string message)
    {
        CurrentStatus = new LocationTrackingStatus
        {
            IsRunning = false,
            SavedPointCount = CurrentStatus.SavedPointCount,
            LastSampleAt = CurrentStatus.LastSampleAt,
            Message = message
        };
    }
}
