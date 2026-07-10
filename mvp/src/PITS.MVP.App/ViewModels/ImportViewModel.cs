using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;

namespace PITS.MVP.App.ViewModels;

public partial class ImportViewModel : BaseViewModel
{
    private readonly IImportService _importService;

    [ObservableProperty] private bool _isImporting;
    [ObservableProperty] private double _importProgress;
    [ObservableProperty] private string _importStatus = "";
    [ObservableProperty] private string _importResult = "";
    [ObservableProperty] private bool _hasPendingItems;

    public ObservableCollection<ImportStagingItem> PendingItems { get; } = new();

    public ImportViewModel(IImportService importService)
    {
        _importService = importService;
        Title = "数据导入";
    }

    public Task LoadPendingAsync() => RefreshPendingAsync();

    [RelayCommand]
    private async Task ImportGoogleTakeoutAsync()
    {
        await StageFileAsync("json", "正在导入 Google Takeout...", stream =>
            _importService.StageGoogleTakeoutAsync(stream, Progress()));
    }

    [RelayCommand]
    private async Task ImportGpxAsync()
    {
        await StageFileAsync("gpx", "正在导入 GPX...", stream =>
            _importService.StageGpxAsync(stream, Progress()));
    }

    [RelayCommand]
    private async Task ImportIcsAsync()
    {
        await StageFileAsync("ics", "正在导入日历计划...", stream =>
            _importService.StageIcsAsync(stream, Progress()));
    }

    [RelayCommand]
    private async Task ImportEmailAsync()
    {
        await StageFileAsync("eml", "正在解析邮件确认单...", stream =>
            _importService.StageEmailAsync(stream, Progress()));
    }

    [RelayCommand]
    private async Task ConfirmAsTripAsync(ImportStagingItem? item)
    {
        if (item == null) return;

        await ExecuteAsync(async () =>
        {
            await _importService.ConfirmStagingItemAsTripAsync(item.Id);
            ImportResult = $"已确认行程：{item.Title}";
            await RefreshPendingAsync();
        });
    }

    [RelayCommand]
    private async Task ConfirmAsPlanAsync(ImportStagingItem? item)
    {
        if (item == null) return;

        await ExecuteAsync(async () =>
        {
            await _importService.ConfirmStagingItemAsPlanAsync(item.Id);
            ImportResult = $"已确认计划：{item.Title}";
            await RefreshPendingAsync();
        });
    }

    [RelayCommand]
    private async Task SkipPendingAsync(ImportStagingItem? item)
    {
        if (item == null) return;

        await ExecuteAsync(async () =>
        {
            await _importService.SkipStagingItemAsync(item.Id);
            ImportResult = $"已忽略：{item.Title}";
            await RefreshPendingAsync();
        });
    }

    private async Task StageFileAsync(string extension, string status, Func<Stream, Task<ImportResult>> stage)
    {
        var filePath = await PickFileAsync(extension);
        if (filePath == null) return;

        IsImporting = true;
        ImportStatus = status;
        ImportProgress = 0;
        ImportResult = "";

        try
        {
            using var stream = File.OpenRead(filePath);
            var result = await stage(stream);
            ImportResult = result.Errors.Any()
                ? $"导入失败：{string.Join(", ", result.Errors)}"
                : $"已加入待确认：{result.ItemsStaged} 项，跳过 {result.PointsSkipped} 项";
        }
        catch (Exception ex)
        {
            ImportResult = $"导入失败：{ex.Message}";
        }
        finally
        {
            IsImporting = false;
            await RefreshPendingAsync();
        }
    }

    private async Task RefreshPendingAsync()
    {
        PendingItems.Clear();
        var pending = await _importService.GetPendingStagingItemsAsync();
        foreach (var item in pending)
        {
            PendingItems.Add(item);
        }

        HasPendingItems = PendingItems.Count > 0;
    }

    private Progress<ImportProgress> Progress() => new(p =>
    {
        ImportProgress = p.Percent / 100;
        ImportStatus = $"已处理 {p.ProcessedPoints}/{p.TotalPoints} 项";
    });

    private static async Task<string?> PickFileAsync(string extension)
    {
        var customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/json", "application/gpx+xml", "text/calendar", "message/rfc822", "text/plain" } },
                { DevicePlatform.iOS, new[] { "public.json", "com.topografix.gpx", "public.ics", "public.email-message", "public.plain-text" } },
                { DevicePlatform.MacCatalyst, new[] { "public.json", "com.topografix.gpx", "public.ics", "public.email-message", "public.plain-text" } },
                { DevicePlatform.WinUI, new[] { ".json", ".gpx", ".ics", ".eml", ".txt" } },
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
