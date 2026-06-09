using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

/// <summary>
/// 基于速度的简单出行方式检测器
/// </summary>
public class TransportModeDetector : ITransportModeDetector
{
    public TransportModeResult DetectMode(IReadOnlyList<TrackPoint> points)
    {
        if (points == null || points.Count < 2)
        {
            return new TransportModeResult { Mode = ActivityType.Other, Confidence = 0 };
        }

        var speeds = new List<double>();
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i - 1].Location == null || points[i].Location == null) continue;

            var timeDiff = (points[i].Timestamp - points[i - 1].Timestamp).TotalSeconds;
            if (timeDiff <= 0) continue;

            var dist = CalculateHaversineDistance(points[i - 1].Location!, points[i].Location!);
            speeds.Add(dist / timeDiff); // m/s
        }

        if (speeds.Count == 0)
        {
            return new TransportModeResult { Mode = ActivityType.Other, Confidence = 0.1 };
        }

        var avgSpeed = speeds.Average();

        // 基于平均速度判断出行方式
        if (avgSpeed < 2) // ~7.2 km/h 步行
        {
            return new TransportModeResult { Mode = ActivityType.Health, Confidence = 0.7 };
        }
        else if (avgSpeed < 8) // ~28.8 km/h 骑行
        {
            return new TransportModeResult { Mode = ActivityType.Personal, Confidence = 0.6 };
        }
        else if (avgSpeed < 25) // ~90 km/h 驾车
        {
            return new TransportModeResult { Mode = ActivityType.Commute, Confidence = 0.7 };
        }
        else // 高速出行
        {
            return new TransportModeResult { Mode = ActivityType.Travel, Confidence = 0.6 };
        }
    }

    private static double CalculateHaversineDistance(Point p1, Point p2)
    {
        var lat1 = p1.Y * Math.PI / 180;
        var lat2 = p2.Y * Math.PI / 180;
        var dLat = (p2.Y - p1.Y) * Math.PI / 180;
        var dLon = (p2.X - p1.X) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return 6371000 * c;
    }
}
