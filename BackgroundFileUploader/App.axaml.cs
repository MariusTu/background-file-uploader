using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Text.Json;
using Avalonia.Controls;

namespace BackgroundFileUploader;

public partial class App : Application
{
    public FolderMonitorService? MonitorService { get; private set; }
    private SettingsWindow? _settingsWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;

            // Load Settings from the runtime JSON file
            AppSettings settings = LoadSettings();

            // --- UPDATE TRAY ICON TOOLTIP ---
            var trayIcons = TrayIcon.GetIcons(this);
            if (trayIcons != null && trayIcons.Count > 0)
            {
                trayIcons[0].ToolTipText = settings.BrandingName;
            }
            // --------------------------------

            // Start the Folder Monitor
            MonitorService = new FolderMonitorService(settings);
            
            // If it fails to start (e.g., empty folder), shut down gracefully
            if (!MonitorService.Start())
            {
                Console.WriteLine("Exiting application.");
                desktop.Shutdown();
                return; // Stop right here
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    // public override void OnFrameworkInitializationCompleted()
    // {
    //     if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    //     {
    //         desktop.MainWindow = new MainWindow();
    //     }

    //     base.OnFrameworkInitializationCompleted();
    // }

    private AppSettings LoadSettings()
    {
        string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        
        // If no file exists, create a default one
        var defaultSettings = new AppSettings();
        File.WriteAllText(settingsPath, JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true }));
        return defaultSettings;
    }

    public void Settings_Clicked(object? sender, EventArgs e)
    {
        // Prevent opening multiple settings windows at once
        if (_settingsWindow == null || !_settingsWindow.IsVisible)
        {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.Show();
        }
        else
        {
            // If it's already open, just bring it to the front
            _settingsWindow.Activate();
        }
    }

    // Handle Tray Icon Exit Click
    public void Exit_Clicked(object? sender, EventArgs e)
    {
        MonitorService?.Stop();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    public void ApplyLiveSettings(AppSettings newSettings)
    {
        // 1. Instantly update the Tray Icon ToolTip
        var trayIcons = TrayIcon.GetIcons(this);
        if (trayIcons != null && trayIcons.Count > 0)
        {
            trayIcons[0].ToolTipText = newSettings.BrandingName;
        }

        // 2. Instantly restart the folder monitor
        MonitorService?.RestartWithNewSettings(newSettings);
    }
}