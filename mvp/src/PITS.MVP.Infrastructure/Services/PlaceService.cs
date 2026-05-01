using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Core.ValueObjects;

namespace PITS.MVP.Infrastructure.Services;

public class PlaceService : IPlaceService
{
    private readonly Data.TripContext _context;
    private static readonly GeometryFactory GeometryFactory = 
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public PlaceService(Data.TripContext context)
    {
        _context = context;
    }

    public async Task<Place?> GetByIdAsync(string id)
    {
        return await _context.Places
            .Include(p => p.Trips)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Place>> GetAllAsync()
    {
        return await _context.Places
            .OrderByDescending(p => p.VisitCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Place>> GetByCategoryAsync(PlaceCategory category)
    {
        return await _context.Places
            .Where(p => p.Category == category)
            .OrderByDescending(p => p.VisitCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Place>> FindNearbyAsync(Point location, double radiusMeters)
    {
        var boundingBox = BoundingBox.FromCenter(location, radiusMeters);
        var polygon = boundingBox.ToPolygon();

        return await _context.Places
            .Where(p => p.Location != null && polygon.Contains(p.Location))
            .OrderByDescending(p => p.VisitCount)
            .ToListAsync();
    }

    public async Task AddAsync(Place place)
    {
        _context.Places.Add(place);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Place place)
    {
        _context.Places.Update(place);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var place = await _context.Places.FindAsync(id);
        if (place != null)
        {
            _context.Places.Remove(place);
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementVisitCountAsync(string placeId)
    {
        var place = await _context.Places.FindAsync(placeId);
        if (place != null)
        {
            place.VisitCount++;
            place.LastVisited = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
