using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Fdb.ViewModels;
using Fdb.Views;

namespace Fdb;

public partial class App : Application
{
    public static bool DisableSplash { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (DisableSplash || desktop.Args?.Contains("--no-splash") == true)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(),
                };
            }
            else
            {
                var splash = new SplashScreenWindow();
                desktop.MainWindow = splash;
                splash.Show();

                _ = Task.Run(async () =>
                {
                    // Simulated 3-second startup sequence
                    await splash.RunStartupSequenceAsync(3000);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var mainWindow = new MainWindow
                        {
                            DataContext = new MainViewModel(),
                        };

                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                        splash.Close();
                    });
                });
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
