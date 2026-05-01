namespace PITS.MVP.App.Views;

public partial class RecordPage : ContentPage
{
    private readonly ViewModels.RecordViewModel _viewModel;

    public RecordPage(ViewModels.RecordViewModel viewModel)
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
