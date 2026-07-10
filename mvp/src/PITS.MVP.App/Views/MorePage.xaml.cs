namespace PITS.MVP.App.Views;

using Microsoft.Extensions.DependencyInjection;

public partial class MorePage : ContentPage
{
    public MorePage()
    {
        InitializeComponent();
    }

    private static Task GoTo<TPage>() where TPage : Page
    {
        var services = Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Application services are unavailable.");
        return Shell.Current.Navigation.PushAsync(services.GetRequiredService<TPage>());
    }

    private void OpenPlaces(object sender, EventArgs e) => _ = GoTo<PlacePage>();
    private void OpenStats(object sender, EventArgs e) => _ = GoTo<StatsPage>();
    private void OpenImport(object sender, EventArgs e) => _ = GoTo<ImportPage>();
    private void OpenSettings(object sender, EventArgs e) => _ = GoTo<SettingsPage>();
}
