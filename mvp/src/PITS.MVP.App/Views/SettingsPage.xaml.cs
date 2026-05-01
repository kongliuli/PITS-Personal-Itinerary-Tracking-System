namespace PITS.MVP.App.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(ViewModels.SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
