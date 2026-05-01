namespace PITS.MVP.Core.ValueObjects;

public record TimeRange(DateTime Start, DateTime End)
{
    public TimeSpan Duration => End - Start;

    public bool Contains(DateTime point) => point >= Start && point <= End;

    public bool Overlaps(TimeRange other) => 
        Start < other.End && other.Start < End;

    public static TimeRange Today => new(
        DateTime.Today, 
        DateTime.Today.AddDays(1).AddTicks(-1));

    public static TimeRange ThisWeek => new(
        DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek),
        DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek).AddTicks(-1));

    public static TimeRange ThisMonth => new(
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 
            DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)).AddTicks(-1));
}
