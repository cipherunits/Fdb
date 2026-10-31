using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Fdb.ViewModels;

public class QueryResultRow
{
    public int RowIndex { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string CreatedAt { get; set; } = string.Empty;
}

public partial class QueryResultTableViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _searchFilter = string.Empty;

    [ObservableProperty]
    private int _totalRows = 5;

    [ObservableProperty]
    private string _executionTime = "14 ms";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private QueryResultRow? _selectedRow;

    public ObservableCollection<QueryResultRow> Rows { get; } = new()
    {
        new QueryResultRow { RowIndex = 1, Id = 101, Name = "Alice Johnson", Email = "alice@example.com", Role = "DBA", Status = "Active", CreatedAt = "2026-01-15 08:30:00" },
        new QueryResultRow { RowIndex = 2, Id = 102, Name = "Bob Smith", Email = "bob@example.com", Role = "Developer", Status = "Active", CreatedAt = "2026-02-10 11:15:22" },
        new QueryResultRow { RowIndex = 3, Id = 103, Name = "Charlie Davis", Email = "charlie@example.com", Role = "Analyst", Status = "Pending", CreatedAt = "2026-03-01 14:45:00" },
        new QueryResultRow { RowIndex = 4, Id = 104, Name = "Dana White", Email = "dana@example.com", Role = "Admin", Status = "Active", CreatedAt = "2026-03-12 09:20:11" },
        new QueryResultRow { RowIndex = 5, Id = 105, Name = "Evan Wright", Email = "evan@example.com", Role = "Developer", Status = "Inactive", CreatedAt = "2026-04-05 16:00:45" }
    };

    [RelayCommand]
    private void Refresh()
    {
        // Ready for refresh query execution
    }

    [RelayCommand]
    private void CopyCell()
    {
    }

    [RelayCommand]
    private void CopyRow()
    {
    }
}
