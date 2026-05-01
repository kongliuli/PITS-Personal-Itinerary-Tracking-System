using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.App.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ITripService _tripService;

    [ObservableProperty] private VisibilityLevel _defaultVisibility = VisibilityLevel.Private;
    [ObservableProperty] private bool _enableBackgroundLocation = true;
    [ObservableProperty] private double _geofenceRadius = 200;

    public List<VisibilityLevel> VisibilityLevels { get; } = Enum.GetValues<VisibilityLevel>().ToList();

    public SettingsViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "设置";
    }

    [RelayCommand]
    private async Task ExportGeoJsonAsync()
    {
        await ExecuteAsync(async () =>
        {
            var trips = await _tripService.GetByVisibilityAsync(VisibilityLevel.Private);
            var geoJson = GenerateGeoJson(trips);
            
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
            var csv = GenerateCsv(trips);
            
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

    private static string GenerateGeoJson(IEnumerable<Trip> trips)
    {
        var features = trips.Where(t => t.Location != null).Select(t => 
            $$"""{"type":"Feature","geometry":{"type":"Point","coordinates":[{{t.Location!.X}},{{t.Location.Y}}]},"properties":{"id":"{{t.Id}}","activity":"{{t.ActivityType}}","description":"{{t.Description}}","startedAt":"{{t.StartedAt:O}}"}}""");
        
        return $$"""{"type":"FeatureCollection","features":[{{string.Join(",", features)}}]}""";
    }

    private static string GenerateCsv(IEnumerable<Trip> trips)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ID,开始时间,结束时间,活动类型,描述,地址,可见性");
        foreach (var t in trips)
        {
            sb.AppendLine($"\"{t.Id}\",\"{t.StartedAt:O}\",\"{t.EndedAt?.ToString("O") ?? ""}\",\"{t.ActivityType}\",\"{t.Description ?? ""}\",\"{t.Address ?? ""}\",\"{t.Visibility}\"");
        }
        return sb.ToString();
    }
}
