using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using System.Collections.ObjectModel;

namespace PITS.MVP.App.ViewModels;

public partial class CalendarViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly ITripPlanService _planService;
    private readonly IReminderService _reminderService;

    [ObservableProperty] private DateTime _currentMonth = DateTime.Today;
    [ObservableProperty] private CalendarDayModel? _selectedDay;
    [ObservableProperty] private string _onThisDaySummary = "";

    public ObservableCollection<CalendarDayModel> CalendarDays { get; } = new();
    public ObservableCollection<Trip> SelectedDayTrips { get; } = new();
    public ObservableCollection<TripPlan> SelectedDayPlans { get; } = new();
    public ObservableCollection<PlanActualRow> SelectedDayComparisons { get; } = new();

    public string CurrentMonthLabel => CurrentMonth.ToString("yyyy年MM月");

    public bool HasSelectedDay => SelectedDay != null;

    public CalendarViewModel(ITripService tripService, ITripPlanService planService, IReminderService reminderService)
    {
        _tripService = tripService;
        _planService = planService;
        _reminderService = reminderService;
        Title = "日历";
    }

    public async Task InitializeAsync()
    {
        await LoadMonthDataAsync();
        await LoadOnThisDayAsync();
    }

    [RelayCommand]
    private async Task PrevMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        await LoadMonthDataAsync();
    }

    [RelayCommand]
    private async Task NextMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        await LoadMonthDataAsync();
    }

    private async Task LoadMonthDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            CalendarDays.Clear();

            var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var startPadding = (int)firstDay.DayOfWeek;
            if (startPadding == 0) startPadding = 7; // 周日排到最后
            startPadding -= 1; // 调整为周一=0

            var trips = await _tripService.GetByDateRangeAsync(
                firstDay.AddDays(-startPadding), 
                lastDay.AddDays(7 - ((int)lastDay.DayOfWeek == 0 ? 7 : (int)lastDay.DayOfWeek)));
            var plans = await _planService.GetByDateRangeAsync(
                firstDay.AddDays(-startPadding),
                lastDay.AddDays(7 - ((int)lastDay.DayOfWeek == 0 ? 7 : (int)lastDay.DayOfWeek)));

            var tripsByDate = trips.GroupBy(t => t.StartedAt.Date).ToDictionary(g => g.Key, g => g.ToList());
            var plansByDate = plans.GroupBy(p => p.StartsAt.Date).ToDictionary(g => g.Key, g => g.ToList());

            for (int i = 0; i < startPadding; i++)
            {
                CalendarDays.Add(new CalendarDayModel { IsCurrentMonth = false });
            }

            for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
            {
                var dayTrips = tripsByDate.GetValueOrDefault(date, new List<Trip>());
                var dayPlans = plansByDate.GetValueOrDefault(date, new List<TripPlan>());
                CalendarDays.Add(new CalendarDayModel
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = true,
                    IsToday = date.Date == DateTime.Today,
                    Trips = dayTrips,
                    Plans = dayPlans,
                    Indicators = dayTrips.Select(t => new TripIndicator(t.ActivityType))
                        .Concat(dayPlans.Select(p => new TripIndicator(p.ActivityType, true)))
                        .ToList()
                });
            }
        });
    }

    [RelayCommand]
    private void SelectDay(CalendarDayModel day)
    {
        if (!day.IsCurrentMonth) return;

        SelectedDay = day;
        SelectedDayTrips.Clear();
        SelectedDayPlans.Clear();
        SelectedDayComparisons.Clear();
        foreach (var trip in day.Trips)
        {
            SelectedDayTrips.Add(trip);
        }
        foreach (var plan in day.Plans)
        {
            SelectedDayPlans.Add(plan);
            var actual = day.Trips
                .Where(t => t.PlanId == plan.Id)
                .OrderBy(t => t.StartedAt)
                .FirstOrDefault();
            SelectedDayComparisons.Add(PlanActualRow.FromPlan(plan, actual));
        }

        var planIds = day.Plans.Select(p => p.Id).ToHashSet();
        foreach (var trip in day.Trips.Where(t => string.IsNullOrWhiteSpace(t.PlanId) || !planIds.Contains(t.PlanId)))
        {
            SelectedDayComparisons.Add(PlanActualRow.FromUnplannedTrip(trip));
        }
        OnPropertyChanged(nameof(HasSelectedDay));
    }

    [RelayCommand]
    private async Task LoadOnThisDayAsync()
    {
        var results = await _reminderService.GetAllOnThisDayAsync();
        if (results.Any())
        {
            OnThisDaySummary = string.Join("\n", results.Select(r => r.Summary));
        }
        else
        {
            OnThisDaySummary = "往年今日没有行程记录";
        }
    }
}

public partial class CalendarDayModel : ObservableObject
{
    public DateTime Date { get; set; }
    public int DayNumber { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public IList<Trip> Trips { get; set; } = new List<Trip>();
    public IList<TripPlan> Plans { get; set; } = new List<TripPlan>();
    public IList<TripIndicator> Indicators { get; set; } = new List<TripIndicator>();

    public Color BorderColor => IsToday ? Colors.Blue : Colors.Transparent;
}

public record TripIndicator(ActivityType ActivityType, bool IsPlan = false)
{
    public Color Color => IsPlan ? Colors.Black : ActivityType switch
    {
        ActivityType.Work => Colors.Blue,
        ActivityType.Commute => Colors.Grey,
        ActivityType.Personal => Colors.Green,
        ActivityType.Travel => Colors.Orange,
        ActivityType.Study => Colors.Purple,
        ActivityType.Health => Colors.Red,
        ActivityType.Entertainment => Colors.Pink,
        _ => Colors.DarkGray
    };
}

public class PlanActualRow
{
    public string PlannedTime { get; set; } = "";
    public string PlannedTitle { get; set; } = "";
    public string ActualTime { get; set; } = "";
    public string ActualTitle { get; set; } = "";
    public string Delta { get; set; } = "";
    public string Status { get; set; } = "";
    public Color StatusColor { get; set; } = Colors.Gray;

    public static PlanActualRow FromPlan(TripPlan plan, Trip? actual)
    {
        if (actual == null)
        {
            var isPast = plan.StartsAt < DateTime.Now;
            return new PlanActualRow
            {
                PlannedTime = plan.StartsAt.ToString("HH:mm"),
                PlannedTitle = plan.Title,
                ActualTime = "-",
                ActualTitle = "未记录",
                Status = isPast ? "未完成" : "待出行",
                StatusColor = isPast ? Colors.Red : Colors.Gray
            };
        }

        var delayMinutes = (int)Math.Round((actual.StartedAt - plan.StartsAt).TotalMinutes);
        return new PlanActualRow
        {
            PlannedTime = plan.StartsAt.ToString("HH:mm"),
            PlannedTitle = plan.Title,
            ActualTime = actual.StartedAt.ToString("HH:mm"),
            ActualTitle = actual.Description ?? actual.ActivityType.ToString(),
            Delta = delayMinutes == 0 ? "准时" : $"{delayMinutes:+#;-#} 分钟",
            Status = delayMinutes > 15 ? "延误" : "完成",
            StatusColor = delayMinutes > 15 ? Colors.OrangeRed : Colors.Green
        };
    }

    public static PlanActualRow FromUnplannedTrip(Trip trip)
    {
        return new PlanActualRow
        {
            PlannedTime = "-",
            PlannedTitle = "未计划",
            ActualTime = trip.StartedAt.ToString("HH:mm"),
            ActualTitle = trip.Description ?? trip.ActivityType.ToString(),
            Status = "实际",
            StatusColor = Colors.DodgerBlue
        };
    }
}
