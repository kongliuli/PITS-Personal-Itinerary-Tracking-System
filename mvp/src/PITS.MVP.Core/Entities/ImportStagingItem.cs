using NetTopologySuite.Geometries;

namespace PITS.MVP.Core.Entities;

public class ImportStagingItem
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public DataSource Source { get; set; } = DataSource.Import;
    public string Fingerprint { get; set; } = "";
    public string? ExternalId { get; set; }

    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string Title { get; set; } = "";
    public string? LocationName { get; set; }
    public Point? Location { get; set; }
    public string? RawPayload { get; set; }
    public ImportStagingStatus Status { get; set; } = ImportStagingStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ConfirmedTripId { get; set; }
    public string? ConfirmedPlanId { get; set; }
}
