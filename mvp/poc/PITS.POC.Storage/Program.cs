using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Infrastructure.Data;

namespace PITS.POC.Storage;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== PITS Storage POC ===\n");

        var dbPath = Path.Combine(Path.GetTempPath(), "pits_poc_test.db");
        if (File.Exists(dbPath)) File.Delete(dbPath);

        var optionsBuilder = new DbContextOptionsBuilder<TripContext>();
        optionsBuilder.UseSqlite($"DataSource={dbPath}");
        optionsBuilder.UseNetTopologySuite();

        await using var context = new TripContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();

        Console.WriteLine($"Database created at: {dbPath}\n");

        Console.WriteLine("--- Test 1: Create Trip ---");
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var trip1 = new Trip
        {
            StartedAt = DateTime.UtcNow,
            EndedAt = DateTime.UtcNow.AddHours(2),
            Location = geometryFactory.CreatePoint(new Coordinate(121.4737, 31.2304)),
            GeoHash = "wtw3s0gf",
            ActivityType = ActivityType.Work,
            Description = "Team standup meeting",
            Visibility = VisibilityLevel.Work,
            Source = DataSource.Manual
        };
        context.Trips.Add(trip1);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created Trip: {trip1.Id}");
        Console.WriteLine($"  Activity: {trip1.ActivityType}");
        Console.WriteLine($"  Location: ({trip1.Location?.Y}, {trip1.Location?.X})");
        Console.WriteLine($"  GeoHash: {trip1.GeoHash}\n");

        Console.WriteLine("--- Test 2: Create Place ---");
        var place1 = new Place
        {
            Name = "Shanghai Tower",
            Category = PlaceCategory.Office,
            Location = geometryFactory.CreatePoint(new Coordinate(121.5014, 31.2355)),
            GeoHash = "wtw3s1g6",
            Radius = 200,
            VisitCount = 5,
            LastVisited = DateTime.UtcNow
        };
        context.Places.Add(place1);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created Place: {place1.Name}");
        Console.WriteLine($"  Category: {place1.Category}");
        Console.WriteLine($"  Visit Count: {place1.VisitCount}\n");

        Console.WriteLine("--- Test 3: Create TrackPoints ---");
        var trackPoints = new List<TrackPoint>();
        for (int i = 0; i < 10; i++)
        {
            trackPoints.Add(new TrackPoint
            {
                Timestamp = DateTime.UtcNow.AddMinutes(-i * 5),
                Location = geometryFactory.CreatePoint(new Coordinate(
                    121.4737 + (i * 0.001),
                    31.2304 + (i * 0.0005))),
                Accuracy = 5.0 + i,
                Speed = 10.0 + (i * 2),
                Altitude = 100.0 + (i * 10)
            });
        }
        context.TrackPoints.AddRange(trackPoints);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {trackPoints.Count} TrackPoints\n");

        Console.WriteLine("--- Test 4: Query All Trips ---");
        var allTrips = await context.Trips.ToListAsync();
        Console.WriteLine($"Total Trips: {allTrips.Count}");
        foreach (var trip in allTrips)
        {
            Console.WriteLine($"  - {trip.Id}: {trip.ActivityType} at {trip.StartedAt:g}");
        }
        Console.WriteLine();

        Console.WriteLine("--- Test 5: Query Trips by Activity Type ---");
        var workTrips = await context.Trips
            .Where(t => t.ActivityType == ActivityType.Work)
            .ToListAsync();
        Console.WriteLine($"Work Trips: {workTrips.Count}\n");

        Console.WriteLine("--- Test 6: Query Places ---");
        var allPlaces = await context.Places.ToListAsync();
        Console.WriteLine($"Total Places: {allPlaces.Count}");
        foreach (var place in allPlaces)
        {
            Console.WriteLine($"  - {place.Name}: {place.Category}");
        }
        Console.WriteLine();

        Console.WriteLine("--- Test 7: Update Trip ---");
        trip1.Description = "Updated standup meeting";
        await context.SaveChangesAsync();
        var updatedTrip = await context.Trips.FindAsync(trip1.Id);
        Console.WriteLine($"Updated Description: {updatedTrip?.Description}\n");

        Console.WriteLine("--- Test 8: Delete Trip ---");
        var tripToDelete = new Trip
        {
            StartedAt = DateTime.UtcNow,
            ActivityType = ActivityType.Personal,
            Description = "To be deleted"
        };
        context.Trips.Add(tripToDelete);
        await context.SaveChangesAsync();
        context.Trips.Remove(tripToDelete);
        await context.SaveChangesAsync();
        var tripCount = await context.Trips.CountAsync();
        Console.WriteLine($"Trips after delete: {tripCount}\n");

        Console.WriteLine("=== All Storage POC Tests Passed ===");

        File.Delete(dbPath);
    }
}
