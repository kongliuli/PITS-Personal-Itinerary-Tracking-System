using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

public interface IPlaceService
{
    Task<Place?> GetByIdAsync(string id);
    Task<IEnumerable<Place>> GetAllAsync();
    Task<IEnumerable<Place>> GetByCategoryAsync(PlaceCategory category);
    Task<IEnumerable<Place>> FindNearbyAsync(Point location, double radiusMeters);
    Task AddAsync(Place place);
    Task UpdateAsync(Place place);
    Task DeleteAsync(string id);
    Task IncrementVisitCountAsync(string placeId);
}
