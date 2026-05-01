using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.App.ViewModels;

public partial class CalendarViewModel : BaseViewModel
{
    private readonly ITripService _tripService;

    [ObservableProperty] private DateTime _currentMonth = DateTime.Today;
    [ObservableProperty] private CalendarDayModel? _selectedDay;
    
    public ObservableCollection<CalendarDayModel> CalendarDays { get; } = new();
    public ObservableCollection<Trip> SelectedDayTrips { get; } = new();

    public string CurrentMonthLabel => CurrentMonth.ToString("yyyy年MM月");

    public bool HasSelectedDay => SelectedDay != null;

    public CalendarViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "日历";
    }

    public async Task InitializeAsync()
    {
        await LoadMonthDataAsync();
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

            var trips = await _tripService.GetByDateRangeAsync(
                firstDay.AddDays(-startPadding), 
                lastDay.AddDays(7 - (int)lastDay.DayOfWeek));

            var tripsByDate = trips.GroupBy(t => t.StartedAt.Date).ToDictionary(g => g.Key, g => g.ToList());

            for (int i = 0; i < startPadding; i++)
            {
                CalendarDays.Add(new CalendarDayModel { IsCurrentMonth = false });
            }

            for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
            {
                var dayTrips = tripsByDate.GetValueOrDefault(date, new List<Trip>());
                CalendarDays.Add(new CalendarDayModel
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = true,
                    IsToday = date.Date == DateTime.Today,
                    Trips = dayTrips,
                    Indicators = dayTrips.Select(t => new TripIndicator(t.ActivityType)).ToList()
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
        foreach (var trip in day.Trips)
        {
            SelectedDayTrips.Add(trip);
        }
        OnPropertyChanged(nameof(HasSelectedDay));
    }
}

public partial class CalendarDayModel : ObservableObject
{
    public DateTime Date { get; set; }
    public int DayNumber { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public IList<Trip> Trips { get; set; } = new List<Trip>();
    public IList<TripIndicator> Indicators { get; set; } = new List<TripIndicator>();

    public Color BorderColor => IsToday ? Colors.Blue : Colors.Transparent;
}

public record TripIndicator(ActivityType ActivityType)
{
    public Color Color => ActivityType switch
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
