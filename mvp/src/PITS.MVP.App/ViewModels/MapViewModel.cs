using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using System.Collections.ObjectModel;

namespace PITS.MVP.App.ViewModels;

public class SpeedColoredSegment
{
    public Color Color { get; set; } = Colors.Blue;
    public List<Location> Locations { get; set; } = new();
    public double SpeedKmh { get; set; }
}

public class HeatmapPoint
{
    public Location Location { get; set; } = null!;
    public double Intensity { get; set; } // 0-1
    public double Radius { get; set; } = 50; // 米
}

public partial class MapViewModel : BaseViewModel
{
    private readonly ITripService _tripService;

    [ObservableProperty] private string _selectedTimeRange = "本周";
    [ObservableProperty] private string _selectedLayer = "全部";
    [ObservableProperty] private bool _useSpeedColors = true;
    [ObservableProperty] private bool _showHeatmap;
    [ObservableProperty] private Trip? _selectedTrip;

    public List<string> TimeOptions { get; } = new() { "今天", "本周", "本月", "全部" };
    public List<string> LayerOptions { get; } = new() { "全部", "公开", "工作", "私人" };

    public List<SpeedColoredSegment> SpeedColoredSegments { get; private set; } = new();
    public ObservableCollection<HeatmapPoint> HeatmapPoints { get; } = new();

    public MapViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "轨迹地图";
    }

    public async Task<IEnumerable<Trip>> GetFilteredTripsAsync()
    {
        var now = DateTime.Now;
        var dayOfWeek = (int)now.DayOfWeek;
        var mondayOffset = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        var (start, end) = SelectedTimeRange switch
        {
            "今天" => (DateTime.Today, DateTime.Today.AddDays(1)),
            "本周" => (now.AddDays(-mondayOffset), now.AddDays(7 - mondayOffset)),
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

    public async Task LoadTrackPointsAsync(string tripId)
    {
        var trackPoints = await _tripService.GetTrackPointsAsync(tripId);
        if (SelectedTrip != null)
        {
            SelectedTrip.TrackPoints = trackPoints.ToList();
        }
        GenerateSpeedColoredSegments();
    }

    partial void OnSelectedTripChanged(Trip? value)
    {
        if (value != null)
        {
            _ = LoadTrackPointsAsync(value.Id);
        }
    }

    partial void OnShowHeatmapChanged(bool value)
    {
        if (value)
        {
            GenerateHeatmapPoints();
        }
    }

    private void GenerateHeatmapPoints()
    {
        HeatmapPoints.Clear();
        if (SelectedTrip?.TrackPoints == null || SelectedTrip.TrackPoints.Count == 0) return;

        // 按 GeoHash 网格聚合
        var grid = new Dictionary<string, List<TrackPoint>>();
        foreach (var point in SelectedTrip.TrackPoints.Where(p => p.Location != null))
        {
            var hash = PITS.MVP.Core.ValueObjects.GeoHash.Encode(point.Location!.Y, point.Location!.X, 6);
            if (!grid.ContainsKey(hash))
                grid[hash] = new List<TrackPoint>();
            grid[hash].Add(point);
        }

        if (grid.Count == 0) return;

        var maxCount = grid.Values.Max(v => v.Count);

        foreach (var kvp in grid)
        {
            var points = kvp.Value;
            var avgLat = points.Average(p => p.Location!.Y);
            var avgLon = points.Average(p => p.Location!.X);
            var intensity = (double)points.Count / maxCount;

            HeatmapPoints.Add(new HeatmapPoint
            {
                Location = new Location(avgLat, avgLon),
                Intensity = intensity,
                Radius = 30 + intensity * 70 // 密度越高圆越大
            });
        }
    }

    private void GenerateSpeedColoredSegments()
    {
        SpeedColoredSegments.Clear();
        if (SelectedTrip?.TrackPoints == null || SelectedTrip.TrackPoints.Count < 2) return;

        var points = SelectedTrip.TrackPoints.OrderBy(p => p.Timestamp).ToList();
        var currentSegment = new SpeedColoredSegment();

        for (int i = 0; i < points.Count; i++)
        {
            double speed = 0;
            if (i > 0 && points[i].Location != null && points[i - 1].Location != null)
            {
                var dist = CalculateDistance(points[i - 1], points[i]);
                var time = (points[i].Timestamp - points[i - 1].Timestamp).TotalSeconds;
                if (time > 0) speed = (dist / time) * 3.6; // km/h
            }

            var color = GetSpeedColor(speed);

            if (currentSegment.Locations.Count > 0 && currentSegment.Color != color)
            {
                // 颜色变化，保存当前段并开始新段
                // 保留最后一个点作为新段的起点
                SpeedColoredSegments.Add(currentSegment);
                var newSegment = new SpeedColoredSegment { Color = color };
                if (currentSegment.Locations.Count > 0)
                    newSegment.Locations.Add(currentSegment.Locations.Last());
                currentSegment = newSegment;
            }
            else
            {
                currentSegment.Color = color;
            }

            if (points[i].Location != null)
            {
                currentSegment.Locations.Add(new Location(points[i].Location.Y, points[i].Location.X));
                currentSegment.SpeedKmh = speed;
            }
        }

        if (currentSegment.Locations.Count > 1)
            SpeedColoredSegments.Add(currentSegment);
    }

    private static Color GetSpeedColor(double speedKmh)
    {
        if (speedKmh < 8) return Colors.DodgerBlue;      // 步行
        if (speedKmh < 25) return Colors.LimeGreen;       // 骑车
        if (speedKmh < 120) return Colors.Orange;         // 驾车
        return Colors.Red;                                  // 高速
    }

    private static double CalculateDistance(TrackPoint p1, TrackPoint p2)
    {
        if (p1.Location == null || p2.Location == null) return 0;
        var lat1 = p1.Location.Y * Math.PI / 180;
        var lat2 = p2.Location.Y * Math.PI / 180;
        var dLat = (p2.Location.Y - p1.Location.Y) * Math.PI / 180;
        var dLon = (p2.Location.X - p1.Location.X) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return 6371000 * c;
    }
}
