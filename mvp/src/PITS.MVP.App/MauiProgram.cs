using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PITS.MVP.Core.Services;
using PITS.MVP.Infrastructure.Data;
using PITS.MVP.Infrastructure.Services;
using PITS.MVP.App.ViewModels;
using PITS.MVP.App.Views;
using Microsoft.EntityFrameworkCore;

namespace PITS.MVP.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                // 使用系统默认字体
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pits_mvp.db");
        builder.Services.AddDbContext<TripContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddScoped<ITripService, TripService>();
        builder.Services.AddScoped<IPlaceService, PlaceService>();
        builder.Services.AddSingleton<IGeocodingService, GeocodingService>();
        builder.Services.AddSingleton<ITransportModeDetector, TransportModeDetector>();
        builder.Services.AddSingleton<ITripSegmentAnalyzer, TripSegmentAnalyzer>();
        builder.Services.AddSingleton<IStatsService, StatsService>();
        builder.Services.AddSingleton<IPlaceClusterService, PlaceClusterService>();
        builder.Services.AddSingleton<IImportService, ImportService>();
        builder.Services.AddSingleton<IReminderService, ReminderService>();
        builder.Services.AddSingleton<IMqttLocationPublisher, MqttLocationPublisher>();
        builder.Services.AddSingleton<ITrackingProfileService, TrackingProfileService>();
        builder.Services.AddSingleton<IPhotoService, PhotoService>();
        builder.Services.AddSingleton<ITripPlanService, TripPlanService>();
        builder.Services.AddSingleton<IPrivacyExportService, PrivacyExportService>();
        builder.Services.AddSingleton<IBackupService, BackupService>();
        builder.Services.AddSingleton<IAlmanacService, AlmanacService>();
#if ANDROID
        builder.Services.AddSingleton<ILocationTrackingService, AndroidLocationTrackingService>();
#else
        builder.Services.AddSingleton<ILocationTrackingService, LocationTrackingService>();
#endif

        builder.Services.AddTransient<RecordPage>();
        builder.Services.AddTransient<RecordViewModel>();
        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<PlacePage>();
        builder.Services.AddTransient<PlaceViewModel>();
        builder.Services.AddTransient<AIChatPage>();
        builder.Services.AddTransient<AIChatViewModel>();
        builder.Services.AddTransient<MorePage>();
        builder.Services.AddTransient<StatsPage>();
        builder.Services.AddTransient<StatsViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<ImportPage>();
        builder.Services.AddTransient<ImportViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        using var scope = app.Services.CreateScope();
        TripContextSchema.EnsureReady(scope.ServiceProvider.GetRequiredService<TripContext>());
        return app;
    }
}
