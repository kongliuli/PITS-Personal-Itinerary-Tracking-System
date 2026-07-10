namespace PITS.MVP.Core.Services;

public interface IAlmanacService
{
    Task<AlmanacDay> GetAsync(DateTime date);
}

public class AlmanacDay
{
    public DateTime Date { get; set; }
    public string Summary { get; set; } = "";
    public string LunarDate { get; set; } = "";
    public string SolarTerm { get; set; } = "";
    public string GoodFor { get; set; } = "";
    public string BadFor { get; set; } = "";
    public string Clash { get; set; } = "";
    public bool IsConfigured { get; set; }
}
