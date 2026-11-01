using System;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fdb.Services;

namespace Fdb.ViewModels;

public partial class SettingsModalViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isOpen;

    [ObservableProperty]
    private string _selectedCategory = "Appearance";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLightTheme))]
    [NotifyPropertyChangedFor(nameof(IsDarkTheme))]
    [NotifyPropertyChangedFor(nameof(IsSystemTheme))]
    private string _selectedTheme = "Light";

    public bool IsLightTheme => string.Equals(SelectedTheme, "Light", StringComparison.OrdinalIgnoreCase);
    public bool IsDarkTheme => string.Equals(SelectedTheme, "Dark", StringComparison.OrdinalIgnoreCase);
    public bool IsSystemTheme => string.Equals(SelectedTheme, "System", StringComparison.OrdinalIgnoreCase);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBlueKeyword))]
    [NotifyPropertyChangedFor(nameof(IsGreenKeyword))]
    [NotifyPropertyChangedFor(nameof(IsPurpleKeyword))]
    [NotifyPropertyChangedFor(nameof(IsAmberKeyword))]
    [NotifyPropertyChangedFor(nameof(IsRoseKeyword))]
    [NotifyPropertyChangedFor(nameof(IsCyanKeyword))]
    [NotifyPropertyChangedFor(nameof(CurrentKeywordBrush))]
    private string _selectedKeywordColor = "Blue";

    public bool IsBlueKeyword => string.Equals(SelectedKeywordColor, "Blue", StringComparison.OrdinalIgnoreCase);
    public bool IsGreenKeyword => string.Equals(SelectedKeywordColor, "Green", StringComparison.OrdinalIgnoreCase);
    public bool IsPurpleKeyword => string.Equals(SelectedKeywordColor, "Purple", StringComparison.OrdinalIgnoreCase);
    public bool IsAmberKeyword => string.Equals(SelectedKeywordColor, "Amber", StringComparison.OrdinalIgnoreCase);
    public bool IsRoseKeyword => string.Equals(SelectedKeywordColor, "Rose", StringComparison.OrdinalIgnoreCase);
    public bool IsCyanKeyword => string.Equals(SelectedKeywordColor, "Cyan", StringComparison.OrdinalIgnoreCase);

    public IBrush CurrentKeywordBrush => ThemeService.Instance.CurrentKeywordBrush;

    private string _initialTheme = "Light";
    private string _initialKeywordColor = "Blue";

    public SettingsModalViewModel()
    {
        _selectedTheme = ThemeService.Instance.CurrentTheme;
        _selectedKeywordColor = ThemeService.Instance.CurrentKeywordColor;
    }

    [RelayCommand]
    public async Task OpenAsync()
    {
        try
        {
            _initialTheme = await DatabaseStorageService.Instance.GetThemeAsync().ConfigureAwait(false);
            SelectedTheme = _initialTheme;

            _initialKeywordColor = await DatabaseStorageService.Instance.GetKeywordColorAsync().ConfigureAwait(false);
            SelectedKeywordColor = _initialKeywordColor;
        }
        catch
        {
            SelectedTheme = ThemeService.Instance.CurrentTheme;
            _initialTheme = SelectedTheme;

            SelectedKeywordColor = ThemeService.Instance.CurrentKeywordColor;
            _initialKeywordColor = SelectedKeywordColor;
        }

        IsOpen = true;
    }

    [RelayCommand]
    public void SelectTheme(string theme)
    {
        if (string.IsNullOrWhiteSpace(theme)) return;
        SelectedTheme = theme;
        // Live preview of the theme (automatically adjusts contrast of current keyword color)
        ThemeService.Instance.ApplyTheme(theme);
        OnPropertyChanged(nameof(CurrentKeywordBrush));
    }

    [RelayCommand]
    public void SelectKeywordColor(string colorKey)
    {
        if (string.IsNullOrWhiteSpace(colorKey)) return;
        SelectedKeywordColor = colorKey;
        // Live preview of keyword color in editor and overlay
        ThemeService.Instance.SetKeywordColor(colorKey);
        OnPropertyChanged(nameof(CurrentKeywordBrush));
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        try
        {
            await DatabaseStorageService.Instance.SaveThemeAsync(SelectedTheme).ConfigureAwait(false);
            await DatabaseStorageService.Instance.SaveKeywordColorAsync(SelectedKeywordColor).ConfigureAwait(false);
            _initialTheme = SelectedTheme;
            _initialKeywordColor = SelectedKeywordColor;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SettingsModalViewModel] Failed to persist settings: {ex.Message}");
        }

        IsOpen = false;
    }

    [RelayCommand]
    public void Close()
    {
        // Revert to initial theme if cancelled without saving
        if (!string.Equals(SelectedTheme, _initialTheme, StringComparison.OrdinalIgnoreCase))
        {
            SelectedTheme = _initialTheme;
            ThemeService.Instance.ApplyTheme(_initialTheme);
        }

        // Revert to initial keyword color if cancelled without saving
        if (!string.Equals(SelectedKeywordColor, _initialKeywordColor, StringComparison.OrdinalIgnoreCase))
        {
            SelectedKeywordColor = _initialKeywordColor;
            ThemeService.Instance.SetKeywordColor(_initialKeywordColor);
        }

        IsOpen = false;
    }
}
