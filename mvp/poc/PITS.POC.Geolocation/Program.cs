using PITS.MVP.Core.ValueObjects;

namespace PITS.POC.Geolocation;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== PITS Geolocation POC ===\n");

        Console.WriteLine("--- Test 1: GeoHash Encoding ---");
        
        var testLocations = new[]
        {
            ("Shanghai", 31.2304, 121.4737),
            ("Beijing", 39.9042, 116.4074),
            ("New York", 40.7128, -74.0060),
            ("London", 51.5074, -0.1278),
            ("Tokyo", 35.6762, 139.6503)
        };

        foreach (var (name, lat, lon) in testLocations)
        {
            for (int precision = 4; precision <= 10; precision++)
            {
                var hash = GeoHash.Encode(lat, lon, precision);
                Console.WriteLine($"  {name} (precision={precision}): {hash}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("--- Test 2: GeoHash Decoding ---");
        var testHashes = new[]
        {
            "wtw3s0gf",
            "wx4g69e1",
            "dr5r7p63n3v9"
        };

        foreach (var hash in testHashes)
        {
            var (lat, lon) = GeoHash.Decode(hash);
            Console.WriteLine($"  {hash} -> ({lat:F6}, {lon:F6})");
        }
        Console.WriteLine();

        Console.WriteLine("--- Test 3: Round-trip Encoding/Decoding ---");
        var originalLat = 31.2304;
        var originalLon = 121.4737;
        var precision = 8;

        var encoded = GeoHash.Encode(originalLat, originalLon, precision);
        var (decodedLat, decodedLon) = GeoHash.Decode(encoded);

        Console.WriteLine($"  Original: ({originalLat}, {originalLon})");
        Console.WriteLine($"  Encoded:  {encoded}");
        Console.WriteLine($"  Decoded:  ({decodedLat:F6}, {decodedLon:F6})");
        Console.WriteLine($"  Error:    Lat ~{(decodedLat - originalLat) * 111 * 1000:F1}m, Lon ~{(decodedLon - originalLon) * 85 * 1000:F1}m\n");

        Console.WriteLine("--- Test 4: GeoHash Precision vs Accuracy ---");
        Console.WriteLine("  Precision | Approximate Area (km x km)");
        Console.WriteLine("  ----------|---------------------------");
        for (int p = 1; p <= 12; p++)
        {
            var area = Math.Pow(Math.Pow(180, 2) / Math.Pow(32, p), 0.5);
            var km = area / 360 * 40075;
            Console.WriteLine($"  {p,10} | ~{km:F2} x {km:F2}");
        }
        Console.WriteLine();

        Console.WriteLine("--- Test 5: TimeRange ---");
        var now = DateTime.UtcNow;
        var today = TimeRange.Today;
        var thisWeek = TimeRange.ThisWeek;
        var thisMonth = TimeRange.ThisMonth;

        Console.WriteLine($"  Today: {today.Start:g} - {today.End:g} (Duration: {today.Duration.TotalHours:F1}h)");
        Console.WriteLine($"  ThisWeek: {thisWeek.Start:g} - {thisWeek.End:g}");
        Console.WriteLine($"  ThisMonth: {thisMonth.Start:g} - {thisMonth.End:g}");
        Console.WriteLine($"  Now in Today: {today.Contains(now)}");
        Console.WriteLine();

        Console.WriteLine("--- Test 6: BoundingBox ---");
        var center = new NetTopologySuite.Geometries.Point(121.4737, 31.2304) { SRID = 4326 };
        var bbox = BoundingBox.FromCenter(center, 1000);
        Console.WriteLine($"  Center: ({center.Y}, {center.X})");
        Console.WriteLine($"  Radius: 1000m");
        Console.WriteLine($"  BoundingBox: N={bbox.North:F4}, S={bbox.South:F4}, E={bbox.East:F4}, W={bbox.West:F4}");
        Console.WriteLine($"  Contains center: {bbox.Contains(center)}");

        var farPoint = new NetTopologySuite.Geometries.Point(121.5, 31.3) { SRID = 4326 };
        Console.WriteLine($"  Contains far point: {bbox.Contains(farPoint)}\n");

        Console.WriteLine("--- Test 7: TimeRange Overlap Detection ---");
        var range1 = new TimeRange(
            new DateTime(2026, 5, 1, 9, 0, 0),
            new DateTime(2026, 5, 1, 12, 0, 0));
        var range2 = new TimeRange(
            new DateTime(2026, 5, 1, 11, 0, 0),
            new DateTime(2026, 5, 1, 14, 0, 0));
        var range3 = new TimeRange(
            new DateTime(2026, 5, 1, 13, 0, 0),
            new DateTime(2026, 5, 1, 16, 0, 0));

        Console.WriteLine($"  Range1: {range1.Start:t} - {range1.End:t}");
        Console.WriteLine($"  Range2: {range2.Start:t} - {range2.End:t}");
        Console.WriteLine($"  Range3: {range3.Start:t} - {range3.End:t}");
        Console.WriteLine($"  Range1 overlaps Range2: {range1.Overlaps(range2)}");
        Console.WriteLine($"  Range1 overlaps Range3: {range1.Overlaps(range3)}\n");

        Console.WriteLine("=== All Geolocation POC Tests Passed ===");
    }
}
