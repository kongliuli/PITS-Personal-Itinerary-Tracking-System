using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

public interface IPlaceClusterService
{
    /// <summary>
    /// 从轨迹点中识别常去地点
    /// </summary>
    Task<List<PlaceCluster>> IdentifyPlacesAsync(IReadOnlyList<TrackPoint> points, double clusterRadiusMeters = 50, int minVisitCount = 3);

    /// <summary>
    /// 自动创建未关联的 Place 记录
    /// </summary>
    Task<int> AutoCreatePlacesAsync();
}

public class PlaceCluster
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int VisitCount { get; set; }
    public double TotalDurationHours { get; set; }
    public string? SuggestedName { get; set; }
}
