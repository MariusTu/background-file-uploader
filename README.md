# Background File Uploader

A lightweight, cross-platform background service built with **Avalonia UI** and **.NET 10**. This application monitors a local folder for new files and securely uploads them to a central Blazor Web API using **HMAC-SHA256** signing for data integrity and security.

## 🚀 Key Features

* **Background Monitoring:** Runs silently in the system tray (Mac & Windows).
* **Secure Uploads:** Every file is signed with a cryptographic HMAC-SHA256 signature to prevent tampering.
* **Live Settings UI:** Right-click the tray icon to open a native settings window.
* **Native Folder Picker:** Easily select monitoring paths without manually editing JSON.
* **Zero Installation:** Available as a self-contained executable; no .NET runtime required on the target machine.
* **Custom Branding:** The tray icon tooltip and app identity can be customized via the settings menu.

## 🛠 Setup & Installation

### Windows
1. Download the latest `BackgroundFileUploader.exe` from the **Releases** section.
2. Double-click to run. Look for the icon in your System Tray (bottom right).
3. Right-click the icon and select **Settings...**.
4. Configure your **API URL**, **Secret Key**, and **Monitoring Folder**.
5. Click **Save**. The app starts watching your folder immediately.

### macOS
1. Download the macOS binary.
2. Run the application. It will appear in your Menu Bar (top right).
3. Configure your settings via the **Settings...** menu.

## ⚙️ Configuration

The application stores settings in a `settings.json` file in its root directory. While you can edit this manually, the built-in UI is recommended to handle path formatting.

Example `settings.json`:
{
  "BrandingName": "CompanyName Uploader",
  "MonitoringFolder": "C:\\Users\\User\\Desktop\\DropZone",
  "ApiUrl": "[https://your-api.com/api/upload](https://your-api.com/api/upload)",
  "ApiSecretKey": "YourVeryLongSecretKey"
}

## 🔒 Security
This app implements a "Zero Trust" upload model:
1. The file is read into a byte array.
2. A Unix timestamp is generated.
3. An HMAC-SHA256 hash is created using the `timestamp + file_content + SecretKey`.
4. The server validates the signature and ensures the timestamp is within a 5-minute window to prevent **Replay Attacks**.

## 🏗 Developer Notes

### Prerequisites
* .NET 10 SDK
* Avalonia UI VS Code Extension (Recommended)

### Build from Source
# Build the solution
dotnet build

# Publish as a single-file executable for Windows
dotnet publish -r win-x64 --self-contained true -p:PublishSingleFile=true
