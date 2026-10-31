using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fdb.ViewModels;

public partial class SidebarViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private string _selectedTab = "Explorer";

    public ObservableCollection<string> Tabs { get; } = new()
    {
        "Explorer",
        "Databases",
        "History"
    };

    [RelayCommand]
    private void SelectTab(string tabName)
    {
        SelectedTab = tabName;
    }

    [RelayCommand]
    private void ToggleVisibility()
    {
        IsVisible = !IsVisible;
    }

    [RelayCommand]
    private void ClosePanel()
    {
        IsVisible = false;
    }

    [RelayCommand]
    private void CloseTab(string tabName)
    {
        if (Tabs.Contains(tabName))
        {
            Tabs.Remove(tabName);
            if (Tabs.Count == 0)
            {
                IsVisible = false;
            }
            else if (SelectedTab == tabName)
            {
                SelectedTab = Tabs.First();
            }
        }
    }
}
