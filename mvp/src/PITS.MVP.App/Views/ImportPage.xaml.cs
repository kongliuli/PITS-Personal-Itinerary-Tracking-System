using PITS.MVP.App.ViewModels;

namespace PITS.MVP.App.Views;

public partial class ImportPage : ContentPage
{
    public ImportPage(ImportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
