using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundFileUploader;

public class UploadService
{
    private readonly AppSettings _settings;
    private static readonly HttpClient _httpClient = new HttpClient();

    public UploadService(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task UploadFileAsync(string filePath)
    {
        try
        {
            // 1. Read the raw bytes of the XML
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            
            // 2. Generate a timestamp to prevent replay attacks
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            
            // 3. Generate the HMAC Signature
            string signature = GenerateHmac(fileBytes, timestamp, _settings.ApiSecretKey);
            
            // 4. Prepare the HTTP POST payload
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            content.Add(fileContent, "file", Path.GetFileName(filePath));

            var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl)
            {
                Content = content
            };

            // 5. Attach the critical metadata and security headers
            request.Headers.Add("X-HMAC-Signature", signature);
            request.Headers.Add("X-Timestamp", timestamp);
            request.Headers.Add("X-Hostname", Environment.MachineName);
            request.Headers.Add("X-Username", Environment.UserName);

            // 6. Send to your Blazor Server
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[{_settings.BrandingName}] Successfully uploaded: {Path.GetFileName(filePath)}");
                // Optional: You can delete the file here after successful upload
                // File.Delete(filePath); 
            }
            else
            {
                Console.WriteLine($"[{_settings.BrandingName}] Server rejected upload. Code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_settings.BrandingName}] Upload Error: {ex.Message}");
        }
    }

    private string GenerateHmac(byte[] payload, string timestamp, string secret)
    {
        // Hash the timestamp + the file bytes together using the Secret Key
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        byte[] timeBytes = Encoding.UTF8.GetBytes(timestamp);
        
        byte[] dataToHash = new byte[timeBytes.Length + payload.Length];
        Buffer.BlockCopy(timeBytes, 0, dataToHash, 0, timeBytes.Length);
        Buffer.BlockCopy(payload, 0, dataToHash, timeBytes.Length, payload.Length);
        
        byte[] hash = hmac.ComputeHash(dataToHash);
        return Convert.ToBase64String(hash);
    }
}