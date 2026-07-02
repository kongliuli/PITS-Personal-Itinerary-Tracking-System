using PITS.MVP.Core.Entities;

namespace PITS.MVP.Core.Services;

public interface IImportService
{
    /// <summary>
    /// 从 Google Takeout JSON 流导入位置历史
    /// </summary>
    Task<ImportResult> ImportFromGoogleTakeoutAsync(Stream jsonStream, IProgress<ImportProgress>? progress = null);

    /// <summary>
    /// 从 GPX 流导入轨迹
    /// </summary>
    Task<ImportResult> ImportFromGpxAsync(Stream gpxStream, IProgress<ImportProgress>? progress = null);

    Task<ImportResult> StageGoogleTakeoutAsync(Stream jsonStream, IProgress<ImportProgress>? progress = null);
    Task<ImportResult> StageGpxAsync(Stream gpxStream, IProgress<ImportProgress>? progress = null);
    Task<ImportResult> StageIcsAsync(Stream icsStream, IProgress<ImportProgress>? progress = null);
    Task<IReadOnlyList<ImportStagingItem>> GetPendingStagingItemsAsync();
    Task<Trip?> ConfirmStagingItemAsTripAsync(string stagingItemId);
    Task<TripPlan?> ConfirmStagingItemAsPlanAsync(string stagingItemId);
}

public class ImportResult
{
    public int PointsImported { get; set; }
    public int TripsCreated { get; set; }
    public int PointsSkipped { get; set; }
    public int ItemsStaged { get; set; }
    public int ItemsConfirmed { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ImportProgress
{
    public int TotalPoints { get; set; }
    public int ProcessedPoints { get; set; }
    public double Percent => TotalPoints > 0 ? (double)ProcessedPoints / TotalPoints * 100 : 0;
}
