using NetTopologySuite.Geometries;

namespace PITS.MVP.Core.Entities;

public class TrackPoint
{
    public long Id { get; set; }
    public string? TripId { get; set; }
    public DateTime Timestamp { get; set; }
    public Point Location { get; set; } = null!;
    public double? Accuracy { get; set; }
    public double? Speed { get; set; }
    public double? Altitude { get; set; }
}
