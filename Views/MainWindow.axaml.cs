using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Fdb.ViewModels;

namespace Fdb.Views;

public partial class MainWindow : Window
{
    private GridLength _lastSidebarWidth = new(240, GridUnitType.Pixel);
    private GridLength _lastFooterHeight = new(140, GridUnitType.Pixel);
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;

        if (SidebarSplitter != null)
        {
            SidebarSplitter.DragCompleted += OnSidebarSplitterDragCompleted;
        }

        if (FooterSplitter != null)
        {
            FooterSplitter.DragCompleted += OnFooterSplitterDragCompleted;
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.Sidebar.PropertyChanged -= OnSidebarPropertyChanged;
            _viewModel.Footer.PropertyChanged -= OnFooterPropertyChanged;
        }

        _viewModel = DataContext as MainViewModel;

        if (_viewModel != null)
        {
            _viewModel.Sidebar.PropertyChanged += OnSidebarPropertyChanged;
            _viewModel.Footer.PropertyChanged += OnFooterPropertyChanged;
            UpdateSidebarLayout();
            UpdateFooterLayout();
        }
    }

    private void OnSidebarPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SidebarViewModel.IsVisible))
        {
            UpdateSidebarLayout();
        }
    }

    private void OnFooterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FooterViewModel.IsVisible) || e.PropertyName == nameof(FooterViewModel.IsMaximized))
        {
            UpdateFooterLayout();
        }
    }

    private void OnSidebarSplitterDragCompleted(object? sender, VectorEventArgs e)
    {
        if (MainGrid == null || MainGrid.ColumnDefinitions.Count < 3) return;

        var col0 = MainGrid.ColumnDefinitions[0];
        if (col0.Width.IsAbsolute && col0.Width.Value >= 160)
        {
            _lastSidebarWidth = col0.Width;
        }

        // Always ensure center workspace remains Star (*) so it dynamically fills space!
        MainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
    }

    private void OnFooterSplitterDragCompleted(object? sender, VectorEventArgs e)
    {
        if (CenterGrid == null || CenterGrid.RowDefinitions.Count < 3) return;

        var row2 = CenterGrid.RowDefinitions[2];
        if (row2.Height.IsAbsolute && row2.Height.Value >= 80)
        {
            _lastFooterHeight = row2.Height;
        }

        // Always ensure document editor remains Star (*) so it dynamically fills space!
        CenterGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
    }

    public void UpdateSidebarLayout()
    {
        if (_viewModel == null || MainGrid == null || MainGrid.ColumnDefinitions.Count < 3) return;
        var col = MainGrid.ColumnDefinitions[0];

        if (_viewModel.Sidebar.IsVisible)
        {
            col.MinWidth = 160;
            col.MaxWidth = 450;
            double w = _lastSidebarWidth.Value;
            if (w < 160) w = 240;
            if (w > 450) w = 450;
            col.Width = new GridLength(w, GridUnitType.Pixel);
        }
        else
        {
            if (col.Width.IsAbsolute && col.Width.Value >= 160)
            {
                _lastSidebarWidth = col.Width;
            }
            col.MinWidth = 0;
            col.Width = new GridLength(0, GridUnitType.Pixel);
        }

        // Ensure center workspace takes all remaining horizontal space
        MainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
    }

    public void UpdateFooterLayout()
    {
        if (_viewModel == null || CenterGrid == null || CenterGrid.RowDefinitions.Count < 3) return;
        var row = CenterGrid.RowDefinitions[2];

        if (_viewModel.Footer.IsVisible)
        {
            if (_viewModel.Footer.IsMaximized)
            {
                // Maximize Footer / Excel to fill entire workspace height
                CenterGrid.RowDefinitions[0].MinHeight = 0;
                CenterGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Pixel);
                CenterGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Pixel);
                if (FooterSplitter != null)
                {
                    FooterSplitter.IsVisible = false;
                }

                row.MinHeight = 0;
                row.MaxHeight = double.PositiveInfinity;
                row.Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                // Restore standard split workspace layout
                CenterGrid.RowDefinitions[0].MinHeight = 180;
                CenterGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                CenterGrid.RowDefinitions[1].Height = GridLength.Auto;
                if (FooterSplitter != null)
                {
                    FooterSplitter.IsVisible = true;
                }

                row.MinHeight = 70;
                row.MaxHeight = 450;
                double h = _lastFooterHeight.Value;
                if (h < 70) h = 140;
                if (h > 450) h = 450;
                row.Height = new GridLength(h, GridUnitType.Pixel);
            }
        }
        else
        {
            if (row.Height.IsAbsolute && row.Height.Value >= 70)
            {
                _lastFooterHeight = row.Height;
            }
            row.MinHeight = 0;
            row.Height = new GridLength(0, GridUnitType.Pixel);
            CenterGrid.RowDefinitions[0].MinHeight = 180;
            CenterGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            if (FooterSplitter != null)
            {
                FooterSplitter.IsVisible = false;
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (_viewModel == null) return;

        // Toggle Fullscreen Table/Footer via F11 or Ctrl+Shift+F
        if (e.Key == Key.F11 || (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.KeyModifiers.HasFlag(KeyModifiers.Shift) && e.Key == Key.F))
        {
            _viewModel.ToggleFooterMaximizedCommand.Execute(null);
            e.Handled = true;
            return;
        }

        // F5: Run active SQL query
        if (e.Key == Key.F5)
        {
            _viewModel.ExecuteQueryCommand.Execute(null);
            e.Handled = true;
            return;
        }

        // Ctrl+S: Save current query
        if (e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
        {
            _viewModel.SaveQueryCommand.Execute(null);
            e.Handled = true;
            return;
        }

        // Ctrl+N: New query tab
        if (e.Key == Key.N && e.KeyModifiers == KeyModifiers.Control)
        {
            _viewModel.Editor.AddTabCommand.Execute(null);
            e.Handled = true;
            return;
        }

        // Ctrl+,: Open Settings
        if (e.Key == Key.OemComma && e.KeyModifiers == KeyModifiers.Control)
        {
            _viewModel.SettingsModal.OpenCommand.Execute(null);
            e.Handled = true;
            return;
        }

        // Ctrl + + / Ctrl + Add: Zoom In SQL Editor Text
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && (e.Key is Key.OemPlus or Key.Add))
        {
            MainEditor?.ZoomIn();
            e.Handled = true;
            return;
        }

        // Ctrl + - / Ctrl + Subtract: Zoom Out SQL Editor Text
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && (e.Key is Key.OemMinus or Key.Subtract))
        {
            MainEditor?.ZoomOut();
            e.Handled = true;
            return;
        }

        // Ctrl + 0: Reset SQL Editor Zoom
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && (e.Key is Key.D0 or Key.NumPad0))
        {
            MainEditor?.ResetZoom();
            e.Handled = true;
            return;
        }

        // Escape dismisses any open modal dialog cleanly, or restores footer if maximized
        if (e.Key == Key.Escape)
        {
            if (_viewModel.ConnectModal.IsOpen)
            {
                _viewModel.ConnectModal.IsOpen = false;
                e.Handled = true;
            }
            else if (_viewModel.ConfirmModal.IsOpen)
            {
                _viewModel.ConfirmModal.IsOpen = false;
                e.Handled = true;
            }
            else if (_viewModel.ExportModal.IsOpen)
            {
                _viewModel.ExportModal.IsOpen = false;
                e.Handled = true;
            }
            else if (_viewModel.SettingsModal.IsOpen)
            {
                _viewModel.SettingsModal.CloseCommand.Execute(null);
                e.Handled = true;
            }
            else if (_viewModel.Footer.IsMaximized)
            {
                _viewModel.Footer.IsMaximized = false;
                e.Handled = true;
            }
        }
    }
}
