using PITS.MVP.Core.Entities;
using PITS.MVP.Core.ValueObjects;

namespace PITS.MVP.Core.Services;

/// <summary>
/// 将 GPS 轨迹点自动分类为停留、出行和数据缺口
/// </summary>
public interface ITripSegmentAnalyzer
{
    /// <summary>
    /// 分析轨迹点并返回分段结果
    /// </summary>
    List<TripSegment> Analyze(IReadOnlyList<TrackPoint> points, double stayRadiusMeters = 50, double stayDurationMinutes = 5, double gapThresholdMinutes = 30);
}
