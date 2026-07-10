using PITS.MVP.App.ViewModels;

namespace PITS.MVP.App.Views;

public partial class ImportPage : ContentPage
{
    private readonly ImportViewModel _viewModel;

    public ImportPage(ImportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadPendingAsync();
    }
}
