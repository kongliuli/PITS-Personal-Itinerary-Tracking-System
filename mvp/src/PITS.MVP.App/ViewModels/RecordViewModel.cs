using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Core.ValueObjects;

namespace PITS.MVP.App.ViewModels;

public partial class RecordViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly IGeocodingService _geoService;

    [ObservableProperty] private string _currentAddress = "正在定位...";
    [ObservableProperty] private string _currentCoords = "";
    [ObservableProperty] private Location? _currentLocation;
    [ObservableProperty] private ActivityType _selectedActivity = ActivityType.Work;
    [ObservableProperty] private VisibilityLevel _selectedVisibility = VisibilityLevel.Private;
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _startTime = DateTime.Now.TimeOfDay;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _endTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromHours(1));

    public ObservableCollection<ActivityTypeModel> ActivityTypes { get; } = new()
    {
        new("🏢", "工作", ActivityType.Work, Colors.Blue),
        new("🚗", "通勤", ActivityType.Commute, Colors.Grey),
        new("☕", "私人", ActivityType.Personal, Colors.Green),
        new("✈️", "出差", ActivityType.Travel, Colors.Orange),
        new("📚", "学习", ActivityType.Study, Colors.Purple),
        new("🏃", "健康", ActivityType.Health, Colors.Red),
    };

    public List<VisibilityLevel> VisibilityLevels { get; } = 
        Enum.GetValues<VisibilityLevel>().ToList();

    public RecordViewModel(ITripService tripService, IGeocodingService geoService)
    {
        _tripService = tripService;
        _geoService = geoService;
        Title = "记录行程";
    }

    public async Task InitializeAsync()
    {
        await ExecuteAsync(async () =>
        {
            var location = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));

            if (location != null)
            {
                CurrentLocation = location;
                CurrentCoords = $"{location.Latitude:F4}, {location.Longitude:F4}";
                CurrentAddress = await _geoService.ReverseGeocodeAsync(
                    location.Latitude, location.Longitude) ?? "未知地点";
            }
            else
            {
                CurrentAddress = "无法获取位置";
            }
        });
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (CurrentLocation == null)
        {
            await Shell.Current.DisplayAlert("错误", "无法获取位置", "确定");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var startedAt = StartDate.Add(StartTime);
            var endedAt = EndDate.Add(EndTime);

            var trip = new Trip
            {
                StartedAt = startedAt,
                EndedAt = endedAt,
                Location = new NetTopologySuite.Geometries.Point(
                    CurrentLocation.Longitude, CurrentLocation.Latitude) { SRID = 4326 },
                GeoHash = GeoHash.Encode(CurrentLocation.Latitude, CurrentLocation.Longitude, 8),
                ActivityType = SelectedActivity,
                Description = Description,
                Visibility = SelectedVisibility,
                Source = DataSource.Manual,
                Accuracy = CurrentLocation.Accuracy,
                Address = CurrentAddress
            };

            await _tripService.AddAsync(trip);

            await Shell.Current.DisplayAlert("成功", "行程已记录", "确定");
            Description = "";
        });
    }
}

public record ActivityTypeModel(string Icon, string Name, ActivityType Type, Color Color);
