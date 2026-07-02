using NetTopologySuite.Geometries;

namespace PITS.MVP.Core.Entities;

public class TripPlan
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string Title { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string Timezone { get; set; } = "Asia/Shanghai";

    public string? LocationName { get; set; }
    public Point? Location { get; set; }
    public string? GeoHash { get; set; }
    public string? Notes { get; set; }

    public ActivityType ActivityType { get; set; } = ActivityType.Other;
    public VisibilityLevel Visibility { get; set; } = VisibilityLevel.Private;
    public DataSource Source { get; set; } = DataSource.Manual;
    public DateTime? ReminderAt { get; set; }
    public PlanStatus Status { get; set; } = PlanStatus.Planned;
    public string? ExternalId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<Trip> ActualTrips { get; set; } = new();
}
