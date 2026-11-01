using Avalonia.Controls;
using Avalonia.Input;
using Fdb.ViewModels;

namespace Fdb.Components;

public partial class SettingsModalComponent : UserControl
{
    public SettingsModalComponent()
    {
        InitializeComponent();
    }

    private void OnBackdropPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.Close();
        }
    }

    private void OnLightCardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.SelectTheme("Light");
        }
    }

    private void OnDarkCardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.SelectTheme("Dark");
        }
    }

    private void OnSystemCardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.SelectTheme("System");
        }
    }

    private void OnBlueColorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.SelectKeywordColor("Blue");
        }
    }

    private void OnGreenColorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.SelectKeywordColor("Green");
        }
    }

    private void OnPurpleColorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.SelectKeywordColor("Purple");
        }
    }

    private void OnAmberColorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.SelectKeywordColor("Amber");
        }
    }

    private void OnRoseColorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.SelectKeywordColor("Rose");
        }
    }

    private void OnCyanColorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SettingsModalViewModel vm)
        {
            vm.SelectKeywordColor("Cyan");
        }
    }
}
