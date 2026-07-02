using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using System.Collections.ObjectModel;

namespace PITS.MVP.App.ViewModels;

public partial class MapViewModel : BaseViewModel
{
    private readonly ITripService _tripService;

    [ObservableProperty] private string _selectedTimeRange = "本周";
    [ObservableProperty] private string _selectedLayer = "全部";
    [ObservableProperty] private string _summary = "";
    [ObservableProperty] private string _emptyMessage = "";
    [ObservableProperty] private bool _isEmpty;

    public List<string> TimeOptions { get; } = new() { "今天", "本周", "本月", "全部" };
    public List<string> LayerOptions { get; } = new() { "全部", "公开", "工作", "私人" };
    public ObservableCollection<MapTripItem> Trips { get; } = new();

    public MapViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "轨迹";
    }

    public async Task LoadAsync()
    {
        await ExecuteAsync(async () =>
        {
            var trips = (await GetFilteredTripsAsync()).ToList();
            Trips.Clear();
            foreach (var trip in trips)
            {
                Trips.Add(MapTripItem.FromTrip(trip));
            }

            var located = trips.Count(t => t.Location != null);
            Summary = $"{trips.Count} 条记录 · {located} 个有坐标";
            IsEmpty = Trips.Count == 0;
            EmptyMessage = IsEmpty ? "当前筛选没有行程记录" : "";
        });
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    partial void OnSelectedTimeRangeChanged(string value) => _ = LoadAsync();
    partial void OnSelectedLayerChanged(string value) => _ = LoadAsync();

    private async Task<IEnumerable<Trip>> GetFilteredTripsAsync()
    {
        var now = DateTime.Now;
        var dayOfWeek = (int)now.DayOfWeek;
        var mondayOffset = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        var (start, end) = SelectedTimeRange switch
        {
            "今天" => (DateTime.Today, DateTime.Today.AddDays(1)),
            "本周" => (DateTime.Today.AddDays(-mondayOffset), DateTime.Today.AddDays(7 - mondayOffset)),
            "本月" => (new DateTime(now.Year, now.Month, 1), DateTime.Today.AddDays(1)),
            _ => (DateTime.MinValue, DateTime.MaxValue)
        };

        var trips = await _tripService.GetByDateRangeAsync(start, end);

        return SelectedLayer switch
        {
            "公开" => trips.Where(t => t.Visibility == VisibilityLevel.Public),
            "工作" => trips.Where(t => t.Visibility <= VisibilityLevel.Work),
            "私人" => trips.Where(t => t.Visibility <= VisibilityLevel.Private),
            _ => trips
        };
    }
}

public class MapTripItem
{
    public string Time { get; set; } = "";
    public string Title { get; set; } = "";
    public string Location { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Badge { get; set; } = "";
    public Color BadgeColor { get; set; } = Colors.Gray;

    public static MapTripItem FromTrip(Trip trip)
    {
        return new MapTripItem
        {
            Time = trip.StartedAt.ToString("MM-dd HH:mm"),
            Title = string.IsNullOrWhiteSpace(trip.Description) ? trip.ActivityType.ToString() : trip.Description,
            Location = trip.Address ?? trip.Place?.Name ?? "未记录地点",
            Detail = trip.EndedAt.HasValue
                ? $"{(int)(trip.EndedAt.Value - trip.StartedAt).TotalMinutes} 分钟"
                : "未记录结束时间",
            Badge = trip.Location == null ? "无坐标" : "有坐标",
            BadgeColor = trip.Location == null ? Colors.Gray : Colors.SeaGreen
        };
    }
}
