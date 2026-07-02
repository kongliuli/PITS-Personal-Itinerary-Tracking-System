using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class PhotoService : IPhotoService
{
    public Task<List<PhotoInfo>> GetPhotosWithLocationAsync(DateTime startDate, DateTime endDate)
    {
        // ponytail: gallery scanning needs a platform adapter when photo import gets UI.
        return Task.FromResult(new List<PhotoInfo>());
    }

    public Task<Dictionary<string, List<PhotoInfo>>> MatchPhotosToTripsAsync(List<PhotoInfo> photos, List<string> tripIds)
    {
        // 按时间匹配照片到行程
        var result = new Dictionary<string, List<PhotoInfo>>();
        foreach (var tripId in tripIds)
        {
            result[tripId] = new List<PhotoInfo>();
        }
        return Task.FromResult(result);
    }
}
