public class AppSettings
{
    public string BrandingName { get; set; } = "Background File Uploader";
    public string MonitoringFolder { get; set; } = "/path/to/monitor";
    public string ApiUrl { get; set; } = "https://your-blazor-server.com/api/upload";
    public string ApiSecretKey { get; set; } = "MySuperSecretKey123";
}