namespace PITS.MVP.App.Views;

public partial class SettingsPage : ContentPage
{
    private readonly ViewModels.SettingsViewModel _viewModel;

    public SettingsPage(ViewModels.SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadStatusAsync();
    }
}
