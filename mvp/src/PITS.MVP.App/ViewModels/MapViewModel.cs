using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.App.ViewModels;

public partial class MapViewModel : BaseViewModel
{
    private readonly ITripService _tripService;

    [ObservableProperty] private string _selectedTimeRange = "本周";
    [ObservableProperty] private string _selectedLayer = "全部";
    
    public List<string> TimeOptions { get; } = new() { "今天", "本周", "本月", "全部" };
    public List<string> LayerOptions { get; } = new() { "全部", "公开", "工作", "私人" };

    public MapViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "轨迹地图";
    }

    public async Task<IEnumerable<Trip>> GetFilteredTripsAsync()
    {
        var now = DateTime.Now;
        var (start, end) = SelectedTimeRange switch
        {
            "今天" => (DateTime.Today, DateTime.Today.AddDays(1)),
            "本周" => (now.AddDays(-(int)now.DayOfWeek), now.AddDays(7 - (int)now.DayOfWeek)),
            "本月" => (new DateTime(now.Year, now.Month, 1), now),
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
