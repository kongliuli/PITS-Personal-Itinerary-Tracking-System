namespace PITS.MVP.App.Views;

public partial class AIChatPage : ContentPage
{
    public AIChatPage(ViewModels.AIChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        Resources.Add("BoolToColorConverter", new Converters.BoolToColorConverter());
        Resources.Add("BoolToLayoutConverter", new Converters.BoolToLayoutConverter());
        Resources.Add("BoolToTextColorConverter", new Converters.BoolToTextColorConverter());
    }
}
