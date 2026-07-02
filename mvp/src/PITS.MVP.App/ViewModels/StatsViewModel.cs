using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.App.ViewModels;

public partial class StatsViewModel : BaseViewModel
{
    private readonly IStatsService _statsService;
    private readonly ITripService _tripService;
    private readonly ITripSegmentAnalyzer _segmentAnalyzer;

    [ObservableProperty]
    private double _totalDistanceKm;

    [ObservableProperty]
    private int _tripCount;

    [ObservableProperty]
    private int _placeCount;

    [ObservableProperty]
    private string _totalDuration = "";

    [ObservableProperty]
    private ObservableCollection<PlaceVisit> _topPlaces = new();

    [ObservableProperty]
    private ObservableCollection<HourBar> _hourDistribution = new();

    [ObservableProperty]
    private ObservableCollection<ActivityItem> _activityDistribution = new();

    [ObservableProperty]
    private int _gapCount;

    [ObservableProperty]
    private string _totalGapDuration = "";

    public StatsViewModel(IStatsService statsService, ITripService tripService, ITripSegmentAnalyzer segmentAnalyzer)
    {
        _statsService = statsService;
        _tripService = tripService;
        _segmentAnalyzer = segmentAnalyzer;
        Title = "统计";
    }

    public async Task LoadStatsAsync()
    {
        await ExecuteAsync(async () =>
        {
            TotalDistanceKm = Math.Round(await _statsService.GetTotalDistanceAsync() / 1000, 1);
            TripCount = await _statsService.GetTripCountAsync();
            PlaceCount = await _statsService.GetPlaceCountAsync();

            var duration = await _statsService.GetTotalDurationAsync();
            TotalDuration = $"{(int)duration.TotalHours}小时{duration.Minutes}分钟";

            var topPlaces = await _statsService.GetTopPlacesAsync(10);
            TopPlaces = new ObservableCollection<PlaceVisit>(topPlaces);

            var hourDist = await _statsService.GetHourDistributionAsync();
            HourDistribution = new ObservableCollection<HourBar>(
                Enumerable.Range(0, 24).Select(h => new HourBar
                {
                    Hour = h,
                    Count = hourDist.GetValueOrDefault(h, 0)
                }));

            var actDist = await _statsService.GetActivityDistributionAsync();
            ActivityDistribution = new ObservableCollection<ActivityItem>(
                actDist.Select(a => new ActivityItem
                {
                    Activity = a.Key.ToString(),
                    Count = a.Value,
                    Color = GetActivityColor(a.Key)
                }));

            // 数据缺口检测
            var trips = await _tripService.GetByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);
            var allGaps = new List<Core.ValueObjects.TripSegment>();
            foreach (var trip in trips)
            {
                var trackPoints = await _tripService.GetTrackPointsAsync(trip.Id);
                if (trackPoints.Count > 0)
                {
                    var segments = _segmentAnalyzer.Analyze(trackPoints);
                    allGaps.AddRange(segments.Where(s => s.Type == Core.ValueObjects.SegmentType.Gap));
                }
            }
            GapCount = allGaps.Count;
            var totalGap = allGaps.Aggregate(TimeSpan.Zero, (acc, g) => acc + g.Duration);
            TotalGapDuration = $"{(int)totalGap.TotalHours}小时{totalGap.Minutes}分钟";
        });
    }

    private static Color GetActivityColor(ActivityType type) => type switch
    {
        ActivityType.Work => Colors.DodgerBlue,
        ActivityType.Commute => Colors.Orange,
        ActivityType.Personal => Colors.MediumPurple,
        ActivityType.Health => Colors.LimeGreen,
        ActivityType.Travel => Colors.Goldenrod,
        ActivityType.Study => Colors.CadetBlue,
        ActivityType.Entertainment => Colors.HotPink,
        _ => Colors.Gray
    };
}

public class HourBar
{
    public int Hour { get; set; }
    public int Count { get; set; }
}

public class ActivityItem
{
    public string Activity { get; set; } = "";
    public int Count { get; set; }
    public Color Color { get; set; } = Colors.Gray;
}
