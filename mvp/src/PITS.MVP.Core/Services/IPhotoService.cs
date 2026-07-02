namespace PITS.MVP.Core.Services;

public interface IPhotoService
{
    /// <summary>
    /// 从设备相册获取带 GPS 信息的照片
    /// </summary>
    Task<List<PhotoInfo>> GetPhotosWithLocationAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 将照片按时间匹配到行程
    /// </summary>
    Task<Dictionary<string, List<PhotoInfo>>> MatchPhotosToTripsAsync(List<PhotoInfo> photos, List<string> tripIds);
}

public class PhotoInfo
{
    public string FilePath { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
    public string? TripId { get; set; }
}
