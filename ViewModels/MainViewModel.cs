using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Fdb.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private NavbarViewModel _navbar;

    [ObservableProperty]
    private SidebarViewModel _sidebar;

    [ObservableProperty]
    private EditorViewModel _editor;

    [ObservableProperty]
    private FooterViewModel _footer;

    [ObservableProperty]
    private ConnectDatabaseModalViewModel _connectModal;

    [ObservableProperty]
    private ConfirmActionModalViewModel _confirmModal;

    [ObservableProperty]
    private ExportDataModalViewModel _exportModal;

    [ObservableProperty]
    private SettingsModalViewModel _settingsModal;

    [ObservableProperty]
    private QueryResultTableViewModel _sampleQueryResult;

    [ObservableProperty]
    private SchemaPropertiesTableViewModel _sampleSchema;

    public MainViewModel()
    {
        _navbar = new NavbarViewModel();
        _sidebar = new SidebarViewModel();
        _editor = new EditorViewModel();
        _footer = new FooterViewModel();
        _connectModal = new ConnectDatabaseModalViewModel();
        _confirmModal = new ConfirmActionModalViewModel();
        _exportModal = new ExportDataModalViewModel();
        _settingsModal = new SettingsModalViewModel();
        _sampleQueryResult = new QueryResultTableViewModel();
        _sampleSchema = new SchemaPropertiesTableViewModel();

        // Initialize SQLite local storage in background
        _ = Fdb.Services.DatabaseStorageService.Instance.InitializeAsync();
    }

    [RelayCommand]
    public async System.Threading.Tasks.Task ExecuteQuery()
    {
        string query = Editor.SelectedTab?.Text ?? "SELECT * FROM dbo.Users;";
        await Fdb.Services.DatabaseStorageService.Instance.SaveQueryHistoryAsync(
            query, "14 ms", SampleQueryResult.Rows.Count, "Success");
        ShowFooter();
    }

    [RelayCommand]
    public async System.Threading.Tasks.Task SaveQuery()
    {
        if (Editor.SelectedTab != null)
        {
            await Fdb.Services.DatabaseStorageService.Instance.SaveQueryHistoryAsync(
                Editor.SelectedTab.Text, "0 ms", 0, "Saved");
        }
    }

    [RelayCommand]
    public void Disconnect()
    {
        ConfirmModal.Show(
            title: "Disconnect Database",
            message: "Are you sure you want to disconnect from localhost:1433? Active queries in tabs will remain preserved.",
            targetName: "localhost:1433 (master)",
            isDestructive: true,
            confirmText: "Disconnect",
            onConfirmed: () =>
            {
                // Disconnected state
            });
    }

    [RelayCommand]
    public void ToggleSidebar()
    {
        Sidebar.IsVisible = !Sidebar.IsVisible;
    }

    [RelayCommand]
    public void ShowSidebar()
    {
        Sidebar.IsVisible = true;
    }

    [RelayCommand]
    public void ToggleFooter()
    {
        Footer.IsVisible = !Footer.IsVisible;
        if (Footer.IsVisible && Footer.Tabs.Count == 0)
        {
            ResetFooterTabs();
        }
    }

    [RelayCommand]
    public void ToggleFooterMaximized()
    {
        Footer.ToggleMaximizeCommand.Execute(null);
    }

    [RelayCommand]
    public void ShowFooter()
    {
        Footer.IsVisible = true;
        if (Footer.Tabs.Count == 0)
        {
            ResetFooterTabs();
        }
    }

    [RelayCommand]
    public void RestoreFooterTab(string tabName)
    {
        Footer.IsVisible = true;
        if (!Footer.Tabs.Contains(tabName))
        {
            Footer.Tabs.Add(tabName);
        }
        Footer.SelectedTab = tabName;
    }

    [RelayCommand]
    public void RestoreSidebarTab(string tabName)
    {
        Sidebar.IsVisible = true;
        if (!Sidebar.Tabs.Contains(tabName))
        {
            Sidebar.Tabs.Add(tabName);
        }
        Sidebar.SelectedTab = tabName;
    }

    [RelayCommand]
    public void ResetLayout()
    {
        // Restore Sidebar
        Sidebar.IsVisible = true;
        Sidebar.Tabs.Clear();
        Sidebar.Tabs.Add("Explorer");
        Sidebar.Tabs.Add("Databases");
        Sidebar.Tabs.Add("History");
        Sidebar.SelectedTab = "Explorer";

        // Restore Footer
        Footer.IsVisible = true;
        ResetFooterTabs();

        // Restore Editor
        if (Editor.Tabs.Count == 0)
        {
            Editor.Tabs.Add(new EditorTabItem { Title = "Tab 1", IsActive = true });
        }
    }

    private void ResetFooterTabs()
    {
        Footer.Tabs.Clear();
        Footer.Tabs.Add("Terminal");
        Footer.Tabs.Add("Output");
        Footer.Tabs.Add("Problems");
        Footer.SelectedTab = "Terminal";
    }

    [RelayCommand]
    public void Exit()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
