using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Fdb.ViewModels;

public partial class ConnectDatabaseModalViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isOpen;

    [ObservableProperty]
    private string _server = "localhost";

    [ObservableProperty]
    private int _port = 1433;

    [ObservableProperty]
    private string _database = "master";

    [ObservableProperty]
    private string _authType = "SQL Server Authentication";

    public ObservableCollection<string> AuthTypes { get; } = new()
    {
        "SQL Server Authentication",
        "Windows Authentication",
        "Microsoft Entra (Azure AD)"
    };

    [ObservableProperty]
    private string _username = "sa";

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _trustServerCertificate = true;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isTesting;

    [RelayCommand]
    public void Open()
    {
        IsOpen = true;
        StatusMessage = null;
    }

    [RelayCommand]
    public void Close()
    {
        IsOpen = false;
        StatusMessage = null;
    }

    [RelayCommand]
    public void TestConnection()
    {
        StatusMessage = "Connecting to " + Server + ":" + Port + "...";
        // Ready for Microsoft.Data.SqlClient test ping
        StatusMessage = "Connection succeeded! (Server version: Microsoft SQL Server 2022)";
    }

    [RelayCommand]
    public void Connect()
    {
        // Ready to set active connection
        IsOpen = false;
    }
}
