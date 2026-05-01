namespace PITS.MVP.App.Views;

public partial class CalendarPage : ContentPage
{
    private readonly ViewModels.CalendarViewModel _viewModel;

    public CalendarPage(ViewModels.CalendarViewModel viewModel)
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
