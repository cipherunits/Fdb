using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Fdb.ViewModels;

namespace Fdb.Views;

public partial class SplashScreenWindow : Window
{
    private bool _isDismissed;
    private readonly TaskCompletionSource<bool> _dismissTcs = new();

    public SplashScreenViewModel ViewModel { get; } = new();

    public SplashScreenWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
        PointerPressed += OnWindowPointerPressed;
        KeyDown += OnWindowKeyDown;
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Dismiss();
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Escape or Key.Space or Key.Enter)
        {
            Dismiss();
        }
    }

    public void Dismiss()
    {
        if (_isDismissed) return;
        _isDismissed = true;
        _dismissTcs.TrySetResult(true);
    }

    public async Task RunStartupSequenceAsync(int durationMs = 3000)
    {
        var vm = ViewModel;
        int steps = 4;
        int delayPerStep = durationMs / steps;

        void SetStep(int s)
        {
            switch (s)
            {
                case 1:
                    vm.StatusText = "Initializing Core Runtime & Workspace...";
                    vm.ProgressValue = 25;
                    break;
                case 2:
                    vm.StatusText = "Loading SQL Keywords, IntelliSense & Syntax Highlighting...";
                    vm.ProgressValue = 50;
                    break;
                case 3:
                    vm.StatusText = "Scanning Database Drivers & Local Instances...";
                    vm.ProgressValue = 75;
                    break;
                case 4:
                    vm.StatusText = "Starting Studio Environment...";
                    vm.ProgressValue = 100;
                    break;
            }
        }

        for (int i = 1; i <= steps; i++)
        {
            if (_isDismissed) break;

            int step = i;
            SetStep(step);

            var delayTask = Task.Delay(delayPerStep);
            var completedTask = await Task.WhenAny(delayTask, _dismissTcs.Task).ConfigureAwait(false);
            if (completedTask == _dismissTcs.Task)
            {
                break;
            }
        }
    }
}
