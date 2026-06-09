using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class PhotoService : IPhotoService
{
    public async Task<List<PhotoInfo>> GetPhotosWithLocationAsync(DateTime startDate, DateTime endDate)
    {
        // TODO: 使用平台特定的 Media API 获取照片
        // MAUI 中需要使用 MediaPicker 或平台特定代码
        // 当前返回空列表作为占位
        return await Task.FromResult(new List<PhotoInfo>());
    }

    public async Task<Dictionary<string, List<PhotoInfo>>> MatchPhotosToTripsAsync(List<PhotoInfo> photos, List<string> tripIds)
    {
        // 按时间匹配照片到行程
        var result = new Dictionary<string, List<PhotoInfo>>();
        foreach (var tripId in tripIds)
        {
            result[tripId] = new List<PhotoInfo>();
        }
        return await Task.FromResult(result);
    }
}
