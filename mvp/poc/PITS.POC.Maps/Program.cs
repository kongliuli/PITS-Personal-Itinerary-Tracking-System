using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace PITS.POC.Maps;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== PITS Maps POC ===\n");

        Console.WriteLine("--- Test 1: Map Span Calculations ---");
        
        var shanghai = new Location(31.2304, 121.4737);
        var beijing = new Location(39.9042, 116.4074);

        var distance = shanghai.CalculateDistance(beijing, DistanceUnits.Kilometers);
        Console.WriteLine($"  Shanghai to Beijing: {distance:F2} km");

        var centerSpan = MapSpan.FromCenterAndRadius(
            shanghai, 
            Distance.FromKilometers(10));
        Console.WriteLine($"  Center: ({centerSpan.Center.Latitude:F4}, {centerSpan.Center.Longitude:F4})");
        Console.WriteLine($"  Radius: {centerSpan.Radius.Kilometers:F2} km\n");

        Console.WriteLine("--- Test 2: Pin Creation ---");
        var pins = new[]
        {
            new Pin { Label = "Office", Address = "123 Main St", Location = shanghai, Type = PinType.Place },
            new Pin { Label = "Home", Address = "456 Oak Ave", Location = new Location(31.2450, 121.4800), Type = PinType.Place },
            new Pin { Label = "Coffee Shop", Address = "789 Elm St", Location = new Location(31.2280, 121.4700), Type = PinType.SearchResult }
        };

        foreach (var pin in pins)
        {
            Console.WriteLine($"  Pin: {pin.Label} at ({pin.Location.Latitude:F4}, {pin.Location.Longitude:F4})");
            Console.WriteLine($"    Address: {pin.Address}");
            Console.WriteLine($"    Type: {pin.Type}");
        }
        Console.WriteLine();

        Console.WriteLine("--- Test 3: Polyline Creation ---");
        var polylinePoints = new[]
        {
            new Location(31.2304, 121.4737),
            new Location(31.2320, 121.4750),
            new Location(31.2340, 121.4770),
            new Location(31.2360, 121.4800),
            new Location(31.2380, 121.4830)
        };

        var polyline = new Polyline
        {
            StrokeColor = Colors.Blue,
            StrokeWidth = 4
        };

        foreach (var point in polylinePoints)
        {
            polyline.Geopath.Add(point);
        }
        Console.WriteLine($"  Polyline with {polyline.Geopath.Count} points created");
        Console.WriteLine($"  Start: ({polyline.Geopath[0].Latitude:F4}, {polyline.Geopath[0].Longitude:F4})");
        Console.WriteLine($"  End: ({polyline.Geopath[^1].Latitude:F4}, {polyline.Geopath[^1].Longitude:F4})\n");

        Console.WriteLine("--- Test 4: Circle Creation (via Polygon) ---");
        var circleCenter = shanghai;
        var circleRadiusKm = 1.0;
        var circlePoints = CreateCirclePolygon(circleCenter, circleRadiusKm);
        Console.WriteLine($"  Circle: Center=({circleCenter.Latitude:F4}, {circleCenter.Longitude:F4})");
        Console.WriteLine($"  Circle: Radius={circleRadiusKm}km, Points={circlePoints.Count}\n");

        Console.WriteLine("--- Test 5: Map Region Bounding ---");
        var allLocations = new[] { shanghai, beijing };
        var boundingRegion = CalculateBoundingRegion(allLocations);
        Console.WriteLine($"  Bounding Region:");
        Console.WriteLine($"    Center: ({boundingRegion.Center.Latitude:F4}, {boundingRegion.Center.Longitude:F4})");
        Console.WriteLine($"    Latitude/Longitude Degrees: {boundingRegion.LatitudeDegrees:F4}x{boundingRegion.LongitudeDegrees:F4}\n");

        Console.WriteLine("=== Maps POC Structure Ready ===");
        Console.WriteLine("Note: Full map UI requires MAUI project running on device/emulator.");
    }

    private static List<Location> CreateCirclePolygon(Location center, double radiusKm)
    {
        var points = new List<Location>();
        var segments = 36;
        var earthRadius = 6371.0;

        for (int i = 0; i <= segments; i++)
        {
            var bearing = 2 * Math.PI * i / segments;
            var lat = center.Latitude * Math.PI / 180;
            var lon = center.Longitude * Math.PI / 180;
            var angularDistance = radiusKm / earthRadius;

            var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angularDistance) +
                Math.Cos(lat) * Math.Sin(angularDistance) * Math.Cos(bearing));
            var newLon = lon + Math.Atan2(
                Math.Sin(bearing) * Math.Sin(angularDistance) * Math.Cos(lat),
                Math.Cos(angularDistance) - Math.Sin(lat) * Math.Sin(newLat));

            points.Add(new Location(newLat * 180 / Math.PI, newLon * 180 / Math.PI));
        }

        return points;
    }

    private static MapSpan CalculateBoundingRegion(Location[] locations)
    {
        if (locations.Length == 0)
            return MapSpan.FromCenterAndRadius(new Location(0, 0), Distance.FromKilometers(1));

        var minLat = locations.Min(l => l.Latitude);
        var maxLat = locations.Max(l => l.Latitude);
        var minLon = locations.Min(l => l.Longitude);
        var maxLon = locations.Max(l => l.Longitude);

        var center = new Location((minLat + maxLat) / 2, (minLon + maxLon) / 2);
        var latDegrees = (maxLat - minLat) / 2 + 0.1;
        var lonDegrees = (maxLon - minLon) / 2 + 0.1;

        return new MapSpan(center, latDegrees, lonDegrees);
    }
}
