using CommunityToolkit.Mvvm.ComponentModel;

namespace Fdb.ViewModels;

public partial class SplashScreenViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _statusText = "Initializing Fdb Studio...";

    [ObservableProperty]
    private double _progressValue = 0;

    [ObservableProperty]
    private string _version = "v1.0.0 Developer Preview";
}
