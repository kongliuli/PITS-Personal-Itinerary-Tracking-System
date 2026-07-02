namespace PITS.MVP.App.Views;

public partial class AIChatPage : ContentPage
{
    public AIChatPage(ViewModels.AIChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
