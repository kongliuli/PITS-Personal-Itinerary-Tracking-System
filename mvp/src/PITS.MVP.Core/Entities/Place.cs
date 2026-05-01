using NetTopologySuite.Geometries;

namespace PITS.MVP.Core.Entities;

public class Place
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string Name { get; set; } = string.Empty;
    public Point? Location { get; set; }
    public string? GeoHash { get; set; }
    public PlaceCategory Category { get; set; }
    public int VisitCount { get; set; }
    public DateTime? LastVisited { get; set; }
    public string? MetadataJson { get; set; }
    public double? Radius { get; set; } = 200;
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
