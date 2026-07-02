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

        builder.Services.AddDbContext<TripContext>(options =>
            options.UseSqlite("DataSource=pits_mvp.db", sqlite => sqlite.UseNetTopologySuite()));

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
        builder.Services.AddTransient<StatsPage>();
        builder.Services.AddTransient<StatsViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<ImportPage>();
        builder.Services.AddTransient<ImportViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
