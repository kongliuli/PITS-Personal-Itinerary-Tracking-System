using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class TripService : ITripService
{
    private readonly Data.TripContext _context;
    private readonly ITransportModeDetector _transportModeDetector;

    public TripService(Data.TripContext context, ITransportModeDetector transportModeDetector)
    {
        _context = context;
        _transportModeDetector = transportModeDetector;
    }

    public async Task<Trip?> GetByIdAsync(string id)
    {
        return await _context.Trips
            .Include(t => t.Place)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Trip>> GetAllAsync()
    {
        return await _context.Trips
            .Include(t => t.Place)
            .OrderByDescending(t => t.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Trip>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Trips
            .Include(t => t.Place)
            .Where(t => t.StartedAt >= start && t.StartedAt <= end)
            .OrderByDescending(t => t.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Trip>> GetByActivityTypeAsync(ActivityType activityType)
    {
        return await _context.Trips
            .Include(t => t.Place)
            .Where(t => t.ActivityType == activityType)
            .OrderByDescending(t => t.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Trip>> GetByVisibilityAsync(VisibilityLevel maxVisibility)
    {
        return await _context.Trips
            .Include(t => t.Place)
            .Where(t => (int)t.Visibility <= (int)maxVisibility)
            .OrderByDescending(t => t.StartedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Trip trip)
    {
        trip.CreatedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
    }

    public async Task<Trip> CreateTripAsync(Trip trip, IReadOnlyList<TrackPoint>? trackPoints = null)
    {
        // 如果传入了轨迹点，自动检测出行方式
        if (trackPoints != null && trackPoints.Count >= 2)
        {
            var modeResult = _transportModeDetector.DetectMode(trackPoints);
            trip.ActivityType = modeResult.Mode ?? ActivityType.Other;

            // 保存轨迹点
            foreach (var tp in trackPoints)
            {
                tp.TripId = trip.Id;
                _context.TrackPoints.Add(tp);
            }
        }

        trip.CreatedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
        return trip;
    }

    public async Task UpdateAsync(Trip trip)
    {
        trip.UpdatedAt = DateTime.UtcNow;
        _context.Trips.Update(trip);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip != null)
        {
            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Trip>> SearchAsync(string query, VisibilityLevel maxVisibility)
    {
        return await _context.Trips
            .Include(t => t.Place)
            .Where(t => (int)t.Visibility <= (int)maxVisibility)
            .Where(t => t.Description != null && t.Description.Contains(query))
            .OrderByDescending(t => t.StartedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrackPoint>> GetTrackPointsAsync(string tripId)
    {
        return await _context.TrackPoints
            .Where(tp => tp.TripId == tripId)
            .OrderBy(tp => tp.Timestamp)
            .ToListAsync();
    }
}
