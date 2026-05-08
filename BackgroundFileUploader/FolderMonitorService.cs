using System;
using System.IO;
using System.Threading.Tasks;

namespace BackgroundFileUploader;

public class FolderMonitorService
{
    private FileSystemWatcher? _watcher;
    private readonly AppSettings _settings;

    private readonly UploadService _uploadService;

    // Inject your white-label settings into the service
    public FolderMonitorService(AppSettings settings)
    {
        _settings = settings;
        _uploadService = new UploadService(settings);
    }

// Change 'void' to 'bool'
    public bool Start()
    {
        // 1. Validate the folder exists
        if (string.IsNullOrWhiteSpace(_settings.MonitoringFolder) || !Directory.Exists(_settings.MonitoringFolder))
        {
            Console.WriteLine($"[{_settings.BrandingName} - Error] Invalid or missing folder: '{_settings.MonitoringFolder}'");
            Console.WriteLine("Please configure your settings.json and restart the app.");
            return false; // Tell the app we failed
        }

        // 2. Configure the watcher
        _watcher = new FileSystemWatcher(_settings.MonitoringFolder)
        {
            Filter = "*.xml",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        // 3. Subscribe to the event
        _watcher.Created += OnFileCreated;
        
        Console.WriteLine($"[{_settings.BrandingName}] Now actively monitoring: {_settings.MonitoringFolder}");
        return true; // Tell the app we succeeded
    }

    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"[Detected] New XML dropped: {e.Name}");
        
        // Wait half a second to ensure the OS has completely finished saving and unlocking the file
        await Task.Delay(500);

        ProcessFile(e.FullPath);
    }

    private async void ProcessFile(string filePath)
    {
        Console.WriteLine($"[Processing] Signing and sending: {filePath}");
        await _uploadService.UploadFileAsync(filePath); // Call the uploader
    }
}