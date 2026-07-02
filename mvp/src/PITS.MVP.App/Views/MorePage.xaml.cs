namespace PITS.MVP.App.Views;

public partial class MorePage : ContentPage
{
    public MorePage()
    {
        InitializeComponent();
    }

    private static Task GoTo(string route) => Shell.Current.GoToAsync(route);

    private void OpenPlaces(object sender, EventArgs e) => _ = GoTo(nameof(PlacePage));
    private void OpenStats(object sender, EventArgs e) => _ = GoTo(nameof(StatsPage));
    private void OpenImport(object sender, EventArgs e) => _ = GoTo(nameof(ImportPage));
    private void OpenSettings(object sender, EventArgs e) => _ = GoTo(nameof(SettingsPage));
}
