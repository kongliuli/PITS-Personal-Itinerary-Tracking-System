using System.Globalization;
using System.Text;
using System.Text.Json;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class PrivacyExportService : IPrivacyExportService
{
    public TripExportRow RedactTrip(Trip trip, VisibilityLevel maxVisibility)
    {
        var allowed = trip.Visibility <= maxVisibility;
        var precision = trip.Visibility switch
        {
            VisibilityLevel.Public => 2,
            VisibilityLevel.Work => 3,
            VisibilityLevel.Private => 4,
            _ => 0
        };

        return new TripExportRow
        {
            Id = trip.Id,
            StartedAt = trip.StartedAt,
            EndedAt = trip.EndedAt,
            ActivityType = trip.ActivityType.ToString(),
            Visibility = trip.Visibility.ToString(),
            Description = allowed ? trip.Description : null,
            Address = allowed && trip.Visibility != VisibilityLevel.Classified ? trip.Address : null,
            Latitude = allowed && trip.Location != null && precision > 0 ? Math.Round(trip.Location.Y, precision) : null,
            Longitude = allowed && trip.Location != null && precision > 0 ? Math.Round(trip.Location.X, precision) : null
        };
    }

    public string ExportCsv(IEnumerable<Trip> trips, VisibilityLevel maxVisibility)
    {
        var rows = trips.Select(t => RedactTrip(t, maxVisibility));
        var sb = new StringBuilder("id,started_at,ended_at,activity,visibility,description,address,latitude,longitude\n");
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", new[]
            {
                Csv(row.Id),
                Csv(row.StartedAt.ToString("O")),
                Csv(row.EndedAt?.ToString("O") ?? ""),
                Csv(row.ActivityType),
                Csv(row.Visibility),
                Csv(row.Description ?? ""),
                Csv(row.Address ?? ""),
                Csv(row.Latitude?.ToString(CultureInfo.InvariantCulture) ?? ""),
                Csv(row.Longitude?.ToString(CultureInfo.InvariantCulture) ?? "")
            }));
        }
        return sb.ToString();
    }

    public string ExportMarkdown(IEnumerable<Trip> trips, VisibilityLevel maxVisibility)
    {
        var sb = new StringBuilder("| Time | Activity | Place | Notes |\n|---|---|---|---|\n");
        foreach (var row in trips.Select(t => RedactTrip(t, maxVisibility)))
        {
            sb.AppendLine($"| {row.StartedAt:g} | {row.ActivityType} | {row.Address ?? ""} | {row.Description ?? ""} |");
        }
        return sb.ToString();
    }

    public string ExportGeoJson(IEnumerable<Trip> trips, VisibilityLevel maxVisibility)
    {
        var features = trips
            .Select(t => RedactTrip(t, maxVisibility))
            .Where(r => r.Latitude != null && r.Longitude != null)
            .Select(r => new
            {
                type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { r.Longitude, r.Latitude } },
                properties = new { r.Id, r.StartedAt, r.ActivityType, r.Visibility, r.Description }
            });

        return JsonSerializer.Serialize(new { type = "FeatureCollection", features });
    }

    private static string Csv(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
