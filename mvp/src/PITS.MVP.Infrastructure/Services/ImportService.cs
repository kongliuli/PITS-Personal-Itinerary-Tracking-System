using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using PITS.MVP.Infrastructure.Data;

namespace PITS.MVP.Infrastructure.Services;

public class ImportService : IImportService
{
    private readonly TripContext _context;

    public ImportService(TripContext context)
    {
        _context = context;
    }

    public async Task<ImportResult> ImportFromGoogleTakeoutAsync(Stream jsonStream, IProgress<ImportProgress>? progress = null)
    {
        var items = await StageGoogleTakeoutItemsAsync(jsonStream, progress);
        return await ConfirmAsTripsAsync(items);
    }

    public async Task<ImportResult> ImportFromGpxAsync(Stream gpxStream, IProgress<ImportProgress>? progress = null)
    {
        var items = await StageGpxItemsAsync(gpxStream, progress);
        return await ConfirmAsTripsAsync(items);
    }

    public async Task<ImportResult> StageGoogleTakeoutAsync(Stream jsonStream, IProgress<ImportProgress>? progress = null)
    {
        var items = await StageGoogleTakeoutItemsAsync(jsonStream, progress);
        return new ImportResult { ItemsStaged = items.Count, PointsImported = items.Count };
    }

    public async Task<ImportResult> StageGpxAsync(Stream gpxStream, IProgress<ImportProgress>? progress = null)
    {
        var items = await StageGpxItemsAsync(gpxStream, progress);
        return new ImportResult { ItemsStaged = items.Count, PointsImported = items.Count };
    }

    public async Task<ImportResult> StageIcsAsync(Stream icsStream, IProgress<ImportProgress>? progress = null)
    {
        using var reader = new StreamReader(icsStream, Encoding.UTF8, leaveOpen: true);
        var content = await reader.ReadToEndAsync();
        var events = ParseIcsEvents(content).ToList();
        var items = await AddStagingItemsAsync(events, progress);
        return new ImportResult { ItemsStaged = items.Count, PointsImported = items.Count };
    }

    public async Task<ImportResult> StageEmailAsync(Stream emailStream, IProgress<ImportProgress>? progress = null)
    {
        using var reader = new StreamReader(emailStream, Encoding.UTF8, leaveOpen: true);
        var content = await reader.ReadToEndAsync();
        var item = ParseEmailConfirmation(content);
        if (item == null)
            return new ImportResult { PointsSkipped = 1, Errors = { "未找到可识别的行程时间" } };

        var items = await AddStagingItemsAsync(new[] { item }, progress);
        return new ImportResult { ItemsStaged = items.Count, PointsImported = items.Count, PointsSkipped = items.Count == 0 ? 1 : 0 };
    }

    public async Task<IReadOnlyList<ImportStagingItem>> GetPendingStagingItemsAsync()
    {
        return await _context.ImportStagingItems
            .Where(i => i.Status == ImportStagingStatus.Pending)
            .OrderBy(i => i.StartsAt)
            .ToListAsync();
    }

    public async Task<Trip?> ConfirmStagingItemAsTripAsync(string stagingItemId)
    {
        var item = await _context.ImportStagingItems.FindAsync(stagingItemId);
        if (item == null || item.Status != ImportStagingStatus.Pending) return null;

        var trip = new Trip
        {
            StartedAt = item.StartsAt,
            EndedAt = item.EndsAt,
            Location = item.Location,
            Address = item.LocationName,
            ActivityType = ActivityType.Other,
            Description = item.Title,
            Visibility = VisibilityLevel.Private,
            Source = item.Source
        };

        item.Status = ImportStagingStatus.Confirmed;
        item.ConfirmedTripId = trip.Id;
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
        return trip;
    }

    public async Task<TripPlan?> ConfirmStagingItemAsPlanAsync(string stagingItemId)
    {
        var item = await _context.ImportStagingItems.FindAsync(stagingItemId);
        if (item == null || item.Status != ImportStagingStatus.Pending) return null;

        var plan = new TripPlan
        {
            Title = item.Title,
            StartsAt = item.StartsAt,
            EndsAt = item.EndsAt,
            LocationName = item.LocationName,
            Location = item.Location,
            ActivityType = ActivityType.Travel,
            Visibility = VisibilityLevel.Private,
            Source = item.Source,
            Status = PlanStatus.Planned,
            ExternalId = item.ExternalId
        };

        item.Status = ImportStagingStatus.Confirmed;
        item.ConfirmedPlanId = plan.Id;
        _context.TripPlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task SkipStagingItemAsync(string stagingItemId)
    {
        var item = await _context.ImportStagingItems.FindAsync(stagingItemId);
        if (item == null || item.Status != ImportStagingStatus.Pending) return;

        item.Status = ImportStagingStatus.Skipped;
        await _context.SaveChangesAsync();
    }

    private async Task<ImportResult> ConfirmAsTripsAsync(IReadOnlyList<ImportStagingItem> items)
    {
        var result = new ImportResult { ItemsStaged = items.Count, PointsImported = items.Count };
        foreach (var item in items)
        {
            var trip = await ConfirmStagingItemAsTripAsync(item.Id);
            if (trip == null)
                result.PointsSkipped++;
            else
                result.TripsCreated++;
        }
        result.ItemsConfirmed = result.TripsCreated;
        return result;
    }

    private async Task<IReadOnlyList<ImportStagingItem>> StageGoogleTakeoutItemsAsync(
        Stream jsonStream,
        IProgress<ImportProgress>? progress)
    {
        var doc = await JsonDocument.ParseAsync(jsonStream);
        if (!doc.RootElement.TryGetProperty("locations", out var locations))
            return Array.Empty<ImportStagingItem>();

        var total = locations.GetArrayLength();
        var items = new List<ImportStagingItem>();
        var processed = 0;
        foreach (var loc in locations.EnumerateArray())
        {
            processed++;
            if (!TryReadGooglePoint(loc, out var item))
                continue;

            items.Add(item);
            progress?.Report(new ImportProgress { TotalPoints = total, ProcessedPoints = processed });
        }

        return await AddStagingItemsAsync(items, progress);
    }

    private async Task<IReadOnlyList<ImportStagingItem>> StageGpxItemsAsync(
        Stream gpxStream,
        IProgress<ImportProgress>? progress)
    {
        var items = new List<ImportStagingItem>();
        using var reader = XmlReader.Create(gpxStream);
        double lat = 0, lon = 0;
        DateTime time = DateTime.MinValue;
        var inPoint = false;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && (reader.Name == "trkpt" || reader.Name == "wpt"))
            {
                inPoint = true;
                lat = double.Parse(reader.GetAttribute("lat") ?? "0");
                lon = double.Parse(reader.GetAttribute("lon") ?? "0");
                time = DateTime.MinValue;
            }
            else if (inPoint && reader.NodeType == XmlNodeType.Element && reader.Name == "time")
            {
                DateTime.TryParse(reader.ReadElementContentAsString(), out time);
            }
            else if (reader.NodeType == XmlNodeType.EndElement && (reader.Name == "trkpt" || reader.Name == "wpt"))
            {
                inPoint = false;
                if (lat == 0 && lon == 0) continue;
                items.Add(CreatePointItem(
                    DataSource.Import,
                    time == DateTime.MinValue ? DateTime.UtcNow : time,
                    lat,
                    lon,
                    "GPX track point",
                    null));
            }
        }

        return await AddStagingItemsAsync(items, progress);
    }

    private async Task<IReadOnlyList<ImportStagingItem>> AddStagingItemsAsync(
        IEnumerable<ImportStagingItem> items,
        IProgress<ImportProgress>? progress = null)
    {
        var staged = new List<ImportStagingItem>();
        var itemList = items.ToList();
        var processed = 0;
        foreach (var item in itemList)
        {
            processed++;
            item.Fingerprint = string.IsNullOrWhiteSpace(item.Fingerprint)
                ? Fingerprint(item.Source, item.StartsAt, item.Title, item.LocationName)
                : item.Fingerprint;

            var exists = await _context.ImportStagingItems.AnyAsync(i => i.Fingerprint == item.Fingerprint);
            if (exists) continue;

            _context.ImportStagingItems.Add(item);
            staged.Add(item);
            progress?.Report(new ImportProgress { TotalPoints = itemList.Count, ProcessedPoints = processed });
        }

        await _context.SaveChangesAsync();
        return staged;
    }

    private static bool TryReadGooglePoint(JsonElement loc, out ImportStagingItem item)
    {
        item = null!;
        var latitude = ReadCoordinate(loc, "latitudeE7", "latitude");
        var longitude = ReadCoordinate(loc, "longitudeE7", "longitude");
        var timestamp = ReadTimestamp(loc);
        if (latitude == null || longitude == null || timestamp == null)
            return false;

        item = CreatePointItem(DataSource.Import, timestamp.Value, latitude.Value, longitude.Value, "Google location", loc.GetRawText());
        return true;
    }

    private static double? ReadCoordinate(JsonElement loc, string e7Name, string decimalName)
    {
        if (loc.TryGetProperty(e7Name, out var e7)) return e7.GetInt64() / 1e7;
        if (loc.TryGetProperty(decimalName, out var dec)) return dec.GetDouble();
        return null;
    }

    private static DateTime? ReadTimestamp(JsonElement loc)
    {
        if (loc.TryGetProperty("timestamp", out var ts) && DateTime.TryParse(ts.GetString(), out var timestamp))
            return timestamp;
        if (loc.TryGetProperty("timestampMs", out var tsMs))
            return DateTimeOffset.FromUnixTimeMilliseconds(tsMs.GetInt64()).UtcDateTime;
        return null;
    }

    private static IEnumerable<ImportStagingItem> ParseIcsEvents(string content)
    {
        foreach (var block in content.Replace("\r\n", "\n").Split("BEGIN:VEVENT").Skip(1))
        {
            var title = ReadIcsValue(block, "SUMMARY") ?? "Calendar event";
            var location = ReadIcsValue(block, "LOCATION");
            var start = ParseIcsDate(ReadIcsValue(block, "DTSTART"));
            if (start == null) continue;

            var end = ParseIcsDate(ReadIcsValue(block, "DTEND"));
            var uid = ReadIcsValue(block, "UID");
            yield return new ImportStagingItem
            {
                Source = DataSource.CalendarSync,
                ExternalId = uid,
                StartsAt = start.Value,
                EndsAt = end,
                Title = title,
                LocationName = location,
                Fingerprint = Fingerprint(DataSource.CalendarSync, start.Value, title, uid ?? location)
            };
        }
    }

    private static ImportStagingItem? ParseEmailConfirmation(string content)
    {
        var normalized = content.Replace("\r\n", "\n");
        var title = ReadHeader(normalized, "Subject") ?? ReadHeader(normalized, "主题") ?? "Email itinerary";
        var start = ReadEmailDate(normalized);
        if (start == null) return null;

        var location = ReadLabeledValue(normalized, "地点")
            ?? ReadLabeledValue(normalized, "地址")
            ?? ReadLabeledValue(normalized, "Location")
            ?? ReadLabeledValue(normalized, "Address");

        return new ImportStagingItem
        {
            Source = DataSource.EmailParse,
            StartsAt = start.Value,
            EndsAt = start.Value.AddHours(1),
            Title = title,
            LocationName = location,
            RawPayload = content,
            Fingerprint = Fingerprint(DataSource.EmailParse, start.Value, title, location)
        };
    }

    private static string? ReadHeader(string content, string name)
    {
        return content.Split('\n')
            .FirstOrDefault(l => l.StartsWith(name + ":", StringComparison.OrdinalIgnoreCase))
            ?.Split(':', 2)[1]
            .Trim();
    }

    private static string? ReadLabeledValue(string content, string name)
    {
        var match = Regex.Match(content, $@"(?:{Regex.Escape(name)})\s*[:：]\s*(.+)");
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static DateTime? ReadEmailDate(string content)
    {
        var match = Regex.Match(content, @"(?<date>\d{4}[-/年]\d{1,2}[-/月]\d{1,2})\D{0,8}(?<time>\d{1,2}:\d{2})?");
        if (!match.Success) return null;

        var value = match.Groups["date"].Value
            .Replace("年", "-")
            .Replace("月", "-")
            .Replace("/", "-")
            .TrimEnd('日');
        if (match.Groups["time"].Success)
            value += " " + match.Groups["time"].Value;

        return DateTime.TryParse(value, out var parsed) ? parsed : null;
    }

    private static string? ReadIcsValue(string block, string name)
    {
        var line = block.Split('\n').FirstOrDefault(l => l.StartsWith(name, StringComparison.OrdinalIgnoreCase));
        if (line == null) return null;
        var colon = line.IndexOf(':');
        return colon < 0 ? null : line[(colon + 1)..].Trim();
    }

    private static DateTime? ParseIcsDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParseExact(value.TrimEnd('Z'), "yyyyMMdd'T'HHmmss", null, System.Globalization.DateTimeStyles.AssumeLocal, out var parsed))
            return parsed;
        if (DateTime.TryParseExact(value, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AssumeLocal, out parsed))
            return parsed;
        return DateTime.TryParse(value, out parsed) ? parsed : null;
    }

    private static ImportStagingItem CreatePointItem(
        DataSource source,
        DateTime timestamp,
        double latitude,
        double longitude,
        string title,
        string? rawPayload)
    {
        return new ImportStagingItem
        {
            Source = source,
            StartsAt = timestamp,
            Title = title,
            Location = new Point(longitude, latitude) { SRID = 4326 },
            RawPayload = rawPayload,
            Fingerprint = Fingerprint(source, timestamp, title, $"{latitude:F6},{longitude:F6}")
        };
    }

    private static string Fingerprint(DataSource source, DateTime startsAt, string title, string? location)
    {
        var input = $"{source}|{startsAt:O}|{title}|{location}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }
}
