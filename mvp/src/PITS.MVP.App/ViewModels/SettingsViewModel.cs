using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.App.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly IPrivacyExportService _privacyExportService;
    private readonly IBackupService _backupService;
    private readonly IMqttLocationPublisher _mqttPublisher;
    private readonly ILocationTrackingService _locationTrackingService;
    private readonly ITrackingProfileService _trackingProfileService;

    [ObservableProperty] private VisibilityLevel _defaultVisibility = VisibilityLevel.Private;
    [ObservableProperty] private bool _enableBackgroundLocation = true;
    [ObservableProperty] private double _geofenceRadius = 200;
    [ObservableProperty] private double _stayRadius = Preferences.Default.Get("stay_radius", 50.0);
    [ObservableProperty] private double _stayDurationMinutes = Preferences.Default.Get("stay_duration_minutes", 5.0);
    [ObservableProperty] private double _gapThresholdMinutes = Preferences.Default.Get("gap_threshold_minutes", 30.0);

    [ObservableProperty] private string _mqttHost = Preferences.Default.Get("mqtt_host", "");

    [ObservableProperty] private int _mqttPort = Preferences.Default.Get("mqtt_port", 1883);

    [ObservableProperty] private string _mqttUsername = Preferences.Default.Get("mqtt_username", "");

    [ObservableProperty] private string _mqttPassword = Preferences.Default.Get("mqtt_password", "");

    [ObservableProperty] private bool _mqttEnabled = Preferences.Default.Get("mqtt_enabled", false);
    [ObservableProperty] private string _almanacApiBaseUrl = Preferences.Default.Get("almanac_api_base_url", "");
    [ObservableProperty] private string _almanacApiKey = Preferences.Default.Get("almanac_api_key", "");
    [ObservableProperty] private string _mqttStatus = "";
    [ObservableProperty] private string _locationTrackingStatus = "v1 后台定位未启动";

    public List<VisibilityLevel> VisibilityLevels { get; } = Enum.GetValues<VisibilityLevel>().ToList();

    partial void OnDefaultVisibilityChanged(VisibilityLevel value)
    {
        Preferences.Default.Set("default_visibility", (int)value);
    }

    partial void OnEnableBackgroundLocationChanged(bool value)
    {
        Preferences.Default.Set("enable_background_location", value);
    }

    partial void OnGeofenceRadiusChanged(double value)
    {
        Preferences.Default.Set("geofence_radius", value);
    }

    partial void OnStayRadiusChanged(double value) => Preferences.Default.Set("stay_radius", value);
    partial void OnStayDurationMinutesChanged(double value) => Preferences.Default.Set("stay_duration_minutes", value);
    partial void OnGapThresholdMinutesChanged(double value) => Preferences.Default.Set("gap_threshold_minutes", value);
    partial void OnMqttHostChanged(string value) => Preferences.Default.Set("mqtt_host", value);
    partial void OnMqttPortChanged(int value) => Preferences.Default.Set("mqtt_port", value);
    partial void OnMqttUsernameChanged(string value) => Preferences.Default.Set("mqtt_username", value);
    partial void OnMqttPasswordChanged(string value) => Preferences.Default.Set("mqtt_password", value);
    partial void OnMqttEnabledChanged(bool value) => Preferences.Default.Set("mqtt_enabled", value);
    partial void OnAlmanacApiBaseUrlChanged(string value) => Preferences.Default.Set("almanac_api_base_url", value);
    partial void OnAlmanacApiKeyChanged(string value) => Preferences.Default.Set("almanac_api_key", value);

    public SettingsViewModel(
        ITripService tripService,
        IPrivacyExportService privacyExportService,
        IBackupService backupService,
        IMqttLocationPublisher mqttPublisher,
        ILocationTrackingService locationTrackingService,
        ITrackingProfileService trackingProfileService)
    {
        _tripService = tripService;
        _privacyExportService = privacyExportService;
        _backupService = backupService;
        _mqttPublisher = mqttPublisher;
        _locationTrackingService = locationTrackingService;
        _trackingProfileService = trackingProfileService;
        Title = "设置";

        // 从 Preferences 恢复设置
        _defaultVisibility = (VisibilityLevel)Preferences.Default.Get("default_visibility", (int)VisibilityLevel.Private);
        _enableBackgroundLocation = Preferences.Default.Get("enable_background_location", true);
        _geofenceRadius = Preferences.Default.Get("geofence_radius", 200.0);
    }

    public async Task LoadStatusAsync()
    {
        var status = await _locationTrackingService.GetStatusAsync();
        var lastSample = status.LastSampleAt.HasValue
            ? $" Last sample: {status.LastSampleAt.Value:MM-dd HH:mm:ss} UTC."
            : "";
        LocationTrackingStatus = $"{status.Message}. Saved points: {status.SavedPointCount}.{lastSample}";
    }

    [RelayCommand]
    private async Task StartLocationTrackingAsync()
    {
        if (!await EnsureLocationPermissionsAsync())
        {
            LocationTrackingStatus = "Location or notification permission denied";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var profile = await _trackingProfileService.GetCurrentProfileAsync();
            await _locationTrackingService.StartAsync(new LocationTrackingOptions
            {
                Enabled = true,
                SampleIntervalSeconds = profile.UpdateIntervalSeconds,
                MinimumDistanceMeters = profile.MinimumDistanceMeters,
                GeofenceRadiusMeters = GeofenceRadius,
                StayRadiusMeters = StayRadius,
                StayDurationMinutes = StayDurationMinutes,
                GapThresholdMinutes = GapThresholdMinutes
            });
            await LoadStatusAsync();
        });

        if (HasError)
            LocationTrackingStatus = $"Location tracking failed: {ErrorMessage}";
    }

    [RelayCommand]
    private async Task StopLocationTrackingAsync()
    {
        await ExecuteAsync(async () =>
        {
            await _locationTrackingService.StopAsync();
            await LoadStatusAsync();
        });
    }

    [RelayCommand]
    private async Task TestMqttConnectionAsync()
    {
        if (string.IsNullOrWhiteSpace(MqttHost))
        {
            MqttStatus = "请先填写 MQTT 服务器地址";
            return;
        }

        await ExecuteAsync(async () =>
        {
            try
            {
                await _mqttPublisher.ConnectAsync(MqttHost, MqttPort, MqttUsername, MqttPassword);
                MqttStatus = _mqttPublisher.IsConnected ? "MQTT 连接成功" : "MQTT 未连接";
            }
            catch (Exception ex)
            {
                MqttStatus = $"MQTT 连接失败：{ex.Message}";
            }
            finally
            {
                await _mqttPublisher.DisconnectAsync();
            }
        });
    }

    [RelayCommand]
    private async Task ExportGeoJsonAsync()
    {
        await ExecuteAsync(async () =>
        {
            var trips = await _tripService.GetByVisibilityAsync(VisibilityLevel.Private);
            var geoJson = _privacyExportService.ExportGeoJson(trips, VisibilityLevel.Private);

            var fileName = $"pits_export_{DateTime.Now:yyyyMMdd_HHmmss}.geojson";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, geoJson);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "导出行程数据",
                File = new ShareFile(filePath)
            });
        });
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        await ExecuteAsync(async () =>
        {
            var trips = await _tripService.GetByVisibilityAsync(VisibilityLevel.Private);
            var csv = _privacyExportService.ExportCsv(trips, VisibilityLevel.Private);

            var fileName = $"pits_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, csv);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "导出行程数据",
                File = new ShareFile(filePath)
            });
        });
    }

    [RelayCommand]
    private async Task ExportGpxAsync()
    {
        await ExecuteAsync(async () =>
        {
            var trips = await _tripService.GetByVisibilityAsync(VisibilityLevel.Private);
            var gpx = GenerateGpx(trips);

            var fileName = $"pits_export_{DateTime.Now:yyyyMMdd_HHmmss}.gpx";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, gpx);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "导出行程数据",
                File = new ShareFile(filePath)
            });
        });
    }

    [RelayCommand]
    private async Task BackupDatabaseAsync()
    {
        await ExecuteAsync(async () =>
        {
            var backupPath = await _backupService.BackupAsync(FileSystem.CacheDirectory);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "备份 PITS 数据库",
                File = new ShareFile(backupPath)
            });
        });
    }

    [RelayCommand]
    private async Task RestoreDatabaseAsync()
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "选择 PITS 数据库备份",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/octet-stream" } },
                { DevicePlatform.iOS, new[] { "public.database", "public.data" } },
                { DevicePlatform.MacCatalyst, new[] { "public.database", "public.data" } },
                { DevicePlatform.WinUI, new[] { ".db" } }
            })
        });
        if (file?.FullPath == null) return;

        await ExecuteAsync(async () =>
        {
            await _backupService.RestoreAsync(file.FullPath);
            await Shell.Current.DisplayAlertAsync("完成", "数据库已恢复，建议重启应用。", "确定");
        });
    }

    private static string GenerateGpx(IEnumerable<Trip> trips)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<gpx version=\"1.1\" creator=\"PITS\">");

        foreach (var trip in trips)
        {
            sb.AppendLine("  <trk>");
            sb.AppendLine($"    <name>{trip.ActivityType} - {trip.StartedAt:yyyy-MM-dd}</name>");
            sb.AppendLine("    <trkseg>");

            if (trip.Location != null)
            {
                sb.AppendLine($"      <trkpt lat=\"{trip.Location.Y}\" lon=\"{trip.Location.X}\">");
                sb.AppendLine($"        <time>{trip.StartedAt:O}</time>");
                sb.AppendLine("      </trkpt>");
            }

            if (trip.TrackPoints != null)
            {
                foreach (var tp in trip.TrackPoints.Where(p => p.Location != null))
                {
                    sb.AppendLine($"      <trkpt lat=\"{tp.Location.Y}\" lon=\"{tp.Location.X}\">");
                    sb.AppendLine($"        <time>{tp.Timestamp:O}</time>");
                    sb.AppendLine("      </trkpt>");
                }
            }

            sb.AppendLine("    </trkseg>");
            sb.AppendLine("  </trk>");
        }

        sb.AppendLine("</gpx>");
        return sb.ToString();
    }

    private static async Task<bool> EnsureLocationPermissionsAsync()
    {
        var location = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (location != PermissionStatus.Granted)
            location = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (location != PermissionStatus.Granted) return false;

#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var notification = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (notification != PermissionStatus.Granted)
                notification = await Permissions.RequestAsync<Permissions.PostNotifications>();
            return notification == PermissionStatus.Granted;
        }
#endif

        return true;
    }
}
