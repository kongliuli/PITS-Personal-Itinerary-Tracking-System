using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.App.ViewModels;

public partial class ImportViewModel : BaseViewModel
{
    private readonly IImportService _importService;

    [ObservableProperty]
    private bool _isImporting;

    [ObservableProperty]
    private double _importProgress;

    [ObservableProperty]
    private string _importStatus = "";

    [ObservableProperty]
    private string _importResult = "";

    public ImportViewModel(IImportService importService)
    {
        _importService = importService;
        Title = "数据导入";
    }

    [RelayCommand]
    private async Task ImportGoogleTakeoutAsync()
    {
        var filePath = await PickFileAsync("json");
        if (filePath == null) return;

        IsImporting = true;
        ImportStatus = "正在导入 Google Takeout...";
        ImportProgress = 0;
        ImportResult = "";

        try
        {
            using var stream = File.OpenRead(filePath);
            var progress = new Progress<ImportProgress>(p =>
            {
                ImportProgress = p.Percent;
                ImportStatus = $"已处理 {p.ProcessedPoints}/{p.TotalPoints} 个位置点";
            });

            var result = await _importService.ImportFromGoogleTakeoutAsync(stream, progress);
            ImportResult = $"导入完成：{result.PointsImported} 个点，{result.TripsCreated} 个行程，{result.PointsSkipped} 个跳过";
            if (result.Errors.Any())
                ImportResult += $"\n错误：{string.Join(", ", result.Errors)}";
        }
        catch (Exception ex)
        {
            ImportResult = $"导入失败：{ex.Message}";
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private async Task ImportGpxAsync()
    {
        var filePath = await PickFileAsync("gpx");
        if (filePath == null) return;

        IsImporting = true;
        ImportStatus = "正在导入 GPX...";
        ImportProgress = 0;
        ImportResult = "";

        try
        {
            using var stream = File.OpenRead(filePath);
            var result = await _importService.ImportFromGpxAsync(stream);
            ImportResult = $"导入完成：{result.PointsImported} 个点，{result.TripsCreated} 个行程，{result.PointsSkipped} 个跳过";
        }
        catch (Exception ex)
        {
            ImportResult = $"导入失败：{ex.Message}";
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private async Task ImportIcsAsync()
    {
        var filePath = await PickFileAsync("ics");
        if (filePath == null) return;

        IsImporting = true;
        ImportStatus = "正在导入日历计划...";
        ImportProgress = 0;
        ImportResult = "";

        try
        {
            using var stream = File.OpenRead(filePath);
            var result = await _importService.StageIcsAsync(stream);
            var pending = await _importService.GetPendingStagingItemsAsync();
            var calendarItems = pending.Where(i => i.Source == DataSource.CalendarSync).ToList();
            foreach (var item in calendarItems)
            {
                await _importService.ConfirmStagingItemAsPlanAsync(item.Id);
            }
            ImportResult = $"导入完成：{result.ItemsStaged} 个日历计划";
        }
        catch (Exception ex)
        {
            ImportResult = $"导入失败：{ex.Message}";
        }
        finally
        {
            IsImporting = false;
        }
    }

    private static async Task<string?> PickFileAsync(string extension)
    {
        var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/json", "application/gpx+xml", "text/calendar" } },
                { DevicePlatform.iOS, new[] { "public.json", "com.topografix.gpx", "public.ics" } },
                { DevicePlatform.MacCatalyst, new[] { "public.json", "com.topografix.gpx", "public.ics" } },
                { DevicePlatform.WinUI, new[] { ".json", ".gpx", ".ics" } },
            });

        var options = new PickOptions
        {
            PickerTitle = $"请选择 {extension.ToUpper()} 文件",
            FileTypes = customFileType,
        };

        var result = await FilePicker.Default.PickAsync(options);
        return result?.FullPath;
    }
}
