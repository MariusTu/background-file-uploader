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
    private FolderMonitorService? _monitorService;

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
            _monitorService = new FolderMonitorService(settings);
            
            // If it fails to start (e.g., empty folder), shut down gracefully
            if (!_monitorService.Start())
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

    // Handle Tray Icon Exit Click
    public void Exit_Clicked(object? sender, EventArgs e)
    {
        _monitorService?.Stop();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}