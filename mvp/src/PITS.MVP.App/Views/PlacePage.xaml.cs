namespace PITS.MVP.App.Views;

public partial class PlacePage : ContentPage
{
    private readonly ViewModels.PlaceViewModel _viewModel;

    public PlacePage(ViewModels.PlaceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
