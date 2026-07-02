using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class TransportModeDetector : ITransportModeDetector
{
    // 速度阈值 (km/h)
    private const double WalkingMax = 8;
    private const double CyclingMax = 25;
    private const double DrivingMax = 120;
    private const double FlyingMin = 200;

    // 公共交通停靠检测
    private const double StopRadius = 50; // 米
    private const double StopDuration = 30; // 秒
    private const int TransitStopThreshold = 3; // 至少3次停靠

    public TransportModeResult DetectMode(IReadOnlyList<TrackPoint> points)
    {
        if (points == null || points.Count < 2)
        {
            return new TransportModeResult
            {
                Mode = ActivityType.Other,
                Confidence = 0,
                AverageSpeedKmh = 0,
                MaxSpeedKmh = 0,
                StopCount = 0
            };
        }

        // 计算速度
        var speeds = new List<double>();
        for (int i = 1; i < points.Count; i++)
        {
            var distance = CalculateDistance(points[i - 1], points[i]);
            var timeDiff = (points[i].Timestamp - points[i - 1].Timestamp).TotalSeconds;
            if (timeDiff > 0)
            {
                var speedKmh = (distance / timeDiff) * 3.6; // m/s -> km/h
                speeds.Add(speedKmh);
            }
        }

        if (speeds.Count == 0)
        {
            return new TransportModeResult
            {
                Mode = ActivityType.Other,
                Confidence = 0,
                AverageSpeedKmh = 0,
                MaxSpeedKmh = 0,
                StopCount = 0
            };
        }

        var avgSpeed = speeds.Average();
        var maxSpeed = speeds.Max();
        var stopCount = CountStops(points);

        // 检测逻辑
        ActivityType mode;
        double confidence;

        if (avgSpeed >= FlyingMin)
        {
            mode = ActivityType.Flying;
            confidence = Math.Min(1.0, avgSpeed / 300);
        }
        else if (avgSpeed < WalkingMax)
        {
            mode = ActivityType.Walking;
            confidence = CalculateConfidence(avgSpeed, 0, WalkingMax);
        }
        else if (avgSpeed < CyclingMax)
        {
            mode = ActivityType.Cycling;
            confidence = CalculateConfidence(avgSpeed, WalkingMax, CyclingMax);
        }
        else if (avgSpeed < DrivingMax)
        {
            // 区分驾车和公共交通
            if (stopCount >= TransitStopThreshold && avgSpeed < 60)
            {
                mode = ActivityType.Transit;
                confidence = Math.Min(1.0, stopCount / 10.0);
            }
            else
            {
                mode = ActivityType.Driving;
                confidence = CalculateConfidence(avgSpeed, CyclingMax, DrivingMax);
            }
        }
        else
        {
            mode = ActivityType.Driving;
            confidence = 0.5; // 高速驾车，可能是高铁
        }

        return new TransportModeResult
        {
            Mode = mode,
            Confidence = confidence,
            AverageSpeedKmh = Math.Round(avgSpeed, 1),
            MaxSpeedKmh = Math.Round(maxSpeed, 1),
            StopCount = stopCount
        };
    }

    private static double CalculateDistance(TrackPoint p1, TrackPoint p2)
    {
        if (p1.Location == null || p2.Location == null) return 0;

        // Haversine 公式
        var lat1 = p1.Location.Y * Math.PI / 180;
        var lat2 = p2.Location.Y * Math.PI / 180;
        var dLat = (p2.Location.Y - p1.Location.Y) * Math.PI / 180;
        var dLon = (p2.Location.X - p1.Location.X) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return 6371000 * c; // 地球半径 * c = 距离(米)
    }

    private static int CountStops(IReadOnlyList<TrackPoint> points)
    {
        int stops = 0;
        int i = 0;
        while (i < points.Count)
        {
            int j = i + 1;
            while (j < points.Count)
            {
                var dist = CalculateDistance(points[i], points[j]);
                var timeDiff = (points[j].Timestamp - points[i].Timestamp).TotalSeconds;
                if (dist > StopRadius || timeDiff > 600) // 超过半径或超过10分钟
                    break;
                j++;
            }

            var duration = j > i + 1 ? (points[j - 1].Timestamp - points[i].Timestamp).TotalSeconds : 0;
            if (duration >= StopDuration)
            {
                stops++;
                i = j;
            }
            else
            {
                i++;
            }
        }
        return stops;
    }

    private static double CalculateConfidence(double value, double rangeStart, double rangeEnd)
    {
        var mid = (rangeStart + rangeEnd) / 2;
        var halfRange = (rangeEnd - rangeStart) / 2;
        return Math.Max(0, 1 - Math.Abs(value - mid) / halfRange);
    }
}
