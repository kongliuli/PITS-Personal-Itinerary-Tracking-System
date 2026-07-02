using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

public interface IPrivacyExportService
{
    TripExportRow RedactTrip(Trip trip, VisibilityLevel maxVisibility);
    string ExportCsv(IEnumerable<Trip> trips, VisibilityLevel maxVisibility);
    string ExportMarkdown(IEnumerable<Trip> trips, VisibilityLevel maxVisibility);
    string ExportGeoJson(IEnumerable<Trip> trips, VisibilityLevel maxVisibility);
}

public class TripExportRow
{
    public string Id { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string ActivityType { get; set; } = "";
    public string Visibility { get; set; } = "";
    public string? Description { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
