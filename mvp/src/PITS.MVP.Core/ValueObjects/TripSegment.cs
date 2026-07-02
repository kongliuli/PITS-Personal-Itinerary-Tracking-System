using PITS.MVP.Core.Entities;
using NetTopologySuite.Geometries;

namespace PITS.MVP.Core.ValueObjects;

public enum SegmentType
{
    Stay,   // 停留
    Trip,   // 出行
    Gap     // 数据缺口
}

public class TripSegment
{
    public SegmentType Type { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Point? CenterLocation { get; set; } // Stay 的中心点
    public List<Point>? Route { get; set; }     // Trip 的路线点
    public TimeSpan Duration => EndTime - StartTime;
    public double DistanceMeters { get; set; }
    public ActivityType? DetectedActivity { get; set; } // Trip 的出行方式
    public Guid? PlaceId { get; set; } // Stay 关联的地点
}
