using NetTopologySuite.Geometries;

namespace PITS.MVP.Core.Entities;

public class Trip
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Timezone { get; set; } = "Asia/Shanghai";

    public Point? Location { get; set; }
    public string? GeoHash { get; set; }
    public double? Accuracy { get; set; }
    public string? Address { get; set; }
    public string? PlaceId { get; set; }

    public ActivityType ActivityType { get; set; }
    public string? Description { get; set; }
    public string? TagsJson { get; set; }

    public VisibilityLevel Visibility { get; set; } = VisibilityLevel.Private;
    public byte[]? EncryptedPayload { get; set; }

    public DataSource Source { get; set; } = DataSource.Manual;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Place? Place { get; set; }
}
