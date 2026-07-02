using Microsoft.Maui;

namespace PITS.MVP.App.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => PITS.MVP.App.MauiProgram.CreateMauiApp();
}
