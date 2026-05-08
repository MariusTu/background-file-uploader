using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Text.Json;

namespace BackgroundFileUploader;

public partial class SettingsWindow : Window
{
    private string _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    public SettingsWindow()
    {
        InitializeComponent();
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                string json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    BrandingTextBox.Text = settings.BrandingName;
                    FolderTextBox.Text = settings.MonitoringFolder;
                    ApiUrlTextBox.Text = settings.ApiUrl;
                    ApiKeyTextBox.Text = settings.ApiSecretKey;
                }
            }
            catch { /* Ignore load errors, textboxes will just be empty */ }
        }
    }

    private async void Browse_Clicked(object? sender, RoutedEventArgs e)
    {
        // Open the native OS folder picker
        var options = new FolderPickerOpenOptions { Title = "Select Folder to Monitor" };
        var result = await StorageProvider.OpenFolderPickerAsync(options);

        if (result.Count > 0)
        {
            // Update the textbox with the selected path
            FolderTextBox.Text = result[0].Path.LocalPath;
        }
    }
    
    private void Save_Clicked(object? sender, RoutedEventArgs e)
    {
        var settings = new AppSettings
        {
            BrandingName = BrandingTextBox.Text ?? "",
            MonitoringFolder = FolderTextBox.Text ?? "",
            ApiUrl = ApiUrlTextBox.Text ?? "",
            ApiSecretKey = ApiKeyTextBox.Text ?? ""
        };

        try
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
            
            // --- NEW LIVE RELOAD CODE ---
            if (Application.Current is App mainApp)
            {
                mainApp.ApplyLiveSettings(settings);
            }
            // ----------------------------

            StatusTextBlock.Foreground = Avalonia.Media.Brushes.Green;
            StatusTextBlock.Text = "Settings saved and applied immediately!";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Foreground = Avalonia.Media.Brushes.Red;
            StatusTextBlock.Text = "Error saving settings.";
            Console.WriteLine($"Save error: {ex.Message}");
        }
    }
}