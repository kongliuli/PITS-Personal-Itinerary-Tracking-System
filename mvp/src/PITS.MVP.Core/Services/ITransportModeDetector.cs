using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

/// <summary>
/// 基于速度和加速度自动检测出行方式
/// </summary>
public interface ITransportModeDetector
{
    /// <summary>
    /// 检测给定轨迹点的出行方式
    /// </summary>
    TransportModeResult DetectMode(IReadOnlyList<TrackPoint> points);
}

/// <summary>
/// 出行方式检测结果
/// </summary>
public class TransportModeResult
{
    public ActivityType? Mode { get; set; }
    public double Confidence { get; set; } // 0-1
    public double AverageSpeedKmh { get; set; }
    public double MaxSpeedKmh { get; set; }
    public int StopCount { get; set; } // 停靠次数（用于区分公交和驾车）
}
