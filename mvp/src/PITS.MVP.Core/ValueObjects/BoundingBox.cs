using NetTopologySuite.Geometries;

namespace PITS.MVP.Core.ValueObjects;

public record BoundingBox(double North, double South, double East, double West)
{
    public Point Center => new((East + West) / 2, (North + South) / 2) { SRID = 4326 };

    public bool Contains(Point point) => 
        point.Y >= South && point.Y <= North && 
        point.X >= West && point.X <= East;

    public Polygon ToPolygon()
    {
        var coordinates = new Coordinate[]
        {
            new(West, South),
            new(East, South),
            new(East, North),
            new(West, North),
            new(West, South)
        };
        return new Polygon(new LinearRing(coordinates)) { SRID = 4326 };
    }

    public static BoundingBox FromCenter(Point center, double radiusMeters)
    {
        const double earthRadius = 6371000;
        double lat = center.Y * Math.PI / 180;
        double dLat = radiusMeters / earthRadius * 180 / Math.PI;
        double dLon = radiusMeters / (earthRadius * Math.Cos(lat)) * 180 / Math.PI;

        return new BoundingBox(
            center.Y + dLat,
            center.Y - dLat,
            center.X + dLon,
            center.X - dLon
        );
    }
}
