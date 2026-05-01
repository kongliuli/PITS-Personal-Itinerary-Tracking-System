using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PITS.MVP.App.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    protected async Task ExecuteAsync(Func<Task> action)
    {
        if (IsBusy) return;
        
        try
        {
            IsBusy = true;
            await action();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
