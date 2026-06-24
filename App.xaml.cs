using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Hardcodet.Wpf.TaskbarNotification;
using Babel.Models;
using Babel.Services;
using Babel.ViewModels;
using Babel.Views;

namespace Babel;

public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private GlobalHotkeyService? _hotkeyService;
    private PopupCoordinator _coordinator = null!;

    private static readonly string[] Greetings =
    [
        "Come, let us build...",
        "The Lord confused the language...",
        "Bridging the gap since the tower fell.",
        "One language was never enough.",
        "Your modern-day Babel fish.",
        "Because not everyone speaks your language. Yet.",
        "Turning babble into meaning.",
        "Let there be understanding.",
        "From Babel, with love.",
    ];

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = AppSettings.Load();

        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton(settings)
                .AddSingleton<IAiModelService, OpenAiService>()
                .AddSingleton<GlobalHotkeyService>()
                .AddSingleton<CaretTrackerService>()
                .AddSingleton<ClipboardTextInjector>()
                .AddSingleton<PopupCoordinator>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<TranslationPopupViewModel>()
                .BuildServiceProvider()
        );

        _coordinator = Ioc.Default.GetRequiredService<PopupCoordinator>();

        if (settings.ActiveProfile == null || string.IsNullOrEmpty(settings.ApiKey))
        {
            var vm = Ioc.Default.GetRequiredService<SettingsViewModel>();
            var settingsWindow = new SettingsWindow(vm);
            var result = settingsWindow.ShowDialog();
            if (result != true)
            {
                Shutdown();
                return;
            }
        }

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        var msg = Greetings[Random.Shared.Next(Greetings.Length)];
        _trayIcon.ShowBalloonTip("Babel", msg, BalloonIcon.Info);

        _hotkeyService = Ioc.Default.GetRequiredService<GlobalHotkeyService>();
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        _hotkeyService.Initialize();
    }

    private void TraySettings_Click(object sender, RoutedEventArgs e)
    {
        _coordinator.CloseCurrentPopup();
        var vm = Ioc.Default.GetRequiredService<SettingsViewModel>();
        var sw = new SettingsWindow(vm);
        sw.ShowDialog();
    }

    private void TrayExit_Click(object sender, RoutedEventArgs e)
    {
        _hotkeyService?.Dispose();
        _trayIcon?.Dispose();
        Shutdown();
    }

    private void OnHotkeyPressed()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var vm = Ioc.Default.GetRequiredService<TranslationPopupViewModel>();
            _coordinator.ShowPopup(vm);
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var settings = Ioc.Default.GetRequiredService<AppSettings>();
        settings.Save();
        _hotkeyService?.Dispose();
        _trayIcon?.Dispose();
        Ioc.Default.GetService<CaretTrackerService>()?.Dispose();
        base.OnExit(e);
    }
}