using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Core.ValueObjects;
using NetTopologySuite.Geometries;

namespace PITS.MVP.Infrastructure.Services;

public class TripSegmentAnalyzer : ITripSegmentAnalyzer
{
    private readonly ITransportModeDetector _modeDetector;

    public TripSegmentAnalyzer(ITransportModeDetector modeDetector)
    {
        _modeDetector = modeDetector;
    }

    public List<TripSegment> Analyze(IReadOnlyList<TrackPoint> points,
        double stayRadiusMeters = 50, double stayDurationMinutes = 5, double gapThresholdMinutes = 30)
    {
        if (points == null || points.Count == 0) return new List<TripSegment>();

        var sorted = points.OrderBy(p => p.Timestamp).ToList();
        var segments = new List<TripSegment>();
        var gapThreshold = TimeSpan.FromMinutes(gapThresholdMinutes);
        var stayDuration = TimeSpan.FromMinutes(stayDurationMinutes);

        int i = 0;
        while (i < sorted.Count)
        {
            // 检测 Gap
            if (i > 0)
            {
                var gap = sorted[i].Timestamp - sorted[i - 1].Timestamp;
                if (gap > gapThreshold)
                {
                    segments.Add(new TripSegment
                    {
                        Type = SegmentType.Gap,
                        StartTime = sorted[i - 1].Timestamp,
                        EndTime = sorted[i].Timestamp
                    });
                }
            }

            // 检测 Stay
            int stayEnd = FindStayEnd(sorted, i, stayRadiusMeters, stayDuration);
            if (stayEnd > i)
            {
                var stayPoints = sorted.Skip(i).Take(stayEnd - i + 1).ToList();
                var center = CalculateCenter(stayPoints);

                segments.Add(new TripSegment
                {
                    Type = SegmentType.Stay,
                    StartTime = stayPoints.First().Timestamp,
                    EndTime = stayPoints.Last().Timestamp,
                    CenterLocation = center,
                    DistanceMeters = 0
                });

                i = stayEnd + 1;
                continue;
            }

            // 检测 Trip（直到下一个 Stay 或 Gap）
            int tripEnd = FindTripEnd(sorted, i, stayRadiusMeters, stayDuration, gapThreshold);
            var tripPoints = sorted.Skip(i).Take(tripEnd - i + 1).ToList();

            if (tripPoints.Count >= 2)
            {
                var mode = _modeDetector.DetectMode(tripPoints);
                var distance = CalculateTotalDistance(tripPoints);

                segments.Add(new TripSegment
                {
                    Type = SegmentType.Trip,
                    StartTime = tripPoints.First().Timestamp,
                    EndTime = tripPoints.Last().Timestamp,
                    Route = tripPoints.Where(p => p.Location != null).Select(p => p.Location!).ToList(),
                    DistanceMeters = distance,
                    DetectedActivity = mode.Mode
                });
            }

            i = tripEnd + 1;
        }

        return segments;
    }

    private static int FindStayEnd(IReadOnlyList<TrackPoint> points, int start, double radiusMeters, TimeSpan minDuration)
    {
        if (points[start].Location == null) return start;

        int end = start;
        for (int j = start + 1; j < points.Count; j++)
        {
            if (points[j].Location == null) break;

            var dist = CalculateHaversineDistance(points[start].Location!, points[j].Location!);
            if (dist > radiusMeters) break;
            end = j;
        }

        var duration = points[end].Timestamp - points[start].Timestamp;
        return duration >= minDuration ? end : start;
    }

    private static int FindTripEnd(IReadOnlyList<TrackPoint> points, int start, double stayRadius, TimeSpan stayDuration, TimeSpan gapThreshold)
    {
        int end = start;
        for (int j = start + 1; j < points.Count; j++)
        {
            // 检查是否进入 Gap
            var gap = points[j].Timestamp - points[j - 1].Timestamp;
            if (gap > gapThreshold) break;

            // 检查是否进入 Stay
            int stayEnd = FindStayEnd(points, j, stayRadius, stayDuration);
            if (stayEnd > j) break;

            end = j;
        }
        return end;
    }

    private static Point CalculateCenter(List<TrackPoint> points)
    {
        var validPoints = points.Where(p => p.Location != null).ToList();
        if (validPoints.Count == 0) return new Point(0, 0);

        var avgX = validPoints.Average(p => p.Location!.X);
        var avgY = validPoints.Average(p => p.Location!.Y);
        return new Point(avgX, avgY) { SRID = 4326 };
    }

    private static double CalculateTotalDistance(List<TrackPoint> points)
    {
        double total = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i - 1].Location != null && points[i].Location != null)
            {
                total += CalculateHaversineDistance(points[i - 1].Location!, points[i].Location!);
            }
        }
        return total;
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
