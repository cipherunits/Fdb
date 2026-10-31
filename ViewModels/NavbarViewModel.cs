using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Fdb.ViewModels;

public partial class NavbarViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Fdb";

    public ObservableCollection<string> MenuItems { get; } = new()
    {
        "File",
        "Edit",
        "View",
        "Query",
        "Help",
        "windows"
    };
}
