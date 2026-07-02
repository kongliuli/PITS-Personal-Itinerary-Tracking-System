using PITS.MVP.App.ViewModels;

namespace PITS.MVP.App.Views;

public partial class StatsPage : ContentPage
{
    public StatsPage(StatsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StatsViewModel vm)
            await vm.LoadStatsAsync();
    }
}
