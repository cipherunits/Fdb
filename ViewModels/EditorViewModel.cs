using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Fdb.ViewModels;

public partial class EditorTabItem : ObservableObject
{
    [ObservableProperty]
    private string _title = "Query 1.sql";

    [ObservableProperty]
    private bool _isActive;

    public TextDocument Document { get; }

    public string Text
    {
        get => Document.Text;
        set
        {
            if (Document.Text != value)
            {
                Document.Text = value;
                OnPropertyChanged();
            }
        }
    }

    public EditorTabItem(string title, string initialText)
    {
        _title = title;
        Document = new TextDocument(initialText);
    }

    public EditorTabItem() : this("Query 1.sql", "-- SQL Query\nSELECT * FROM dbo.Users;\n")
    {
    }
}

public partial class EditorViewModel : ViewModelBase
{
    [ObservableProperty]
    private EditorTabItem? _selectedTab;

    public ObservableCollection<EditorTabItem> Tabs { get; } = new();

    public EditorViewModel()
    {
        var tab1 = new EditorTabItem
        {
            Title = "Query 1.sql",
            Text = "-- Query 1: Retrieve active users\nSELECT user_id, username, email \nFROM dbo.Users \nWHERE is_active = 1 \nORDER BY created_at DESC;\n",
            IsActive = true
        };

        var tab2 = new EditorTabItem
        {
            Title = "Query 2.sql",
            Text = "-- Query 2: System catalog query\nSELECT name, create_date, modify_date \nFROM sys.tables \nORDER BY name ASC;\n",
            IsActive = false
        };

        Tabs.Add(tab1);
        Tabs.Add(tab2);
        _selectedTab = tab1;
    }

    [RelayCommand]
    public void SelectTab(EditorTabItem tab)
    {
        if (tab == null) return;
        foreach (var t in Tabs)
        {
            t.IsActive = (t == tab);
        }
        SelectedTab = tab;
    }

    [RelayCommand]
    public void AddTab()
    {
        int nextIndex = Tabs.Count + 1;
        var newTab = new EditorTabItem
        {
            Title = $"Query {nextIndex}.sql",
            Text = $"-- Query {nextIndex}\nSELECT TOP 100 * \nFROM sys.objects \nWHERE type = 'U';\n",
            IsActive = false
        };
        Tabs.Add(newTab);
        SelectTab(newTab);
    }

    [RelayCommand]
    public void CloseTab(EditorTabItem tab)
    {
        if (Tabs.Contains(tab))
        {
            bool wasActive = (SelectedTab == tab);
            Tabs.Remove(tab);
            if (Tabs.Count == 0)
            {
                AddTab();
            }
            else if (wasActive)
            {
                SelectTab(Tabs.Last());
            }
        }
    }

    [RelayCommand]
    public void CloseOtherTabs(EditorTabItem keepTab)
    {
        var toRemove = Tabs.Where(t => t != keepTab).ToList();
        foreach (var t in toRemove)
        {
            Tabs.Remove(t);
        }
        SelectTab(keepTab);
    }

    [RelayCommand]
    public void CloseAllTabs()
    {
        Tabs.Clear();
        AddTab();
    }
}
