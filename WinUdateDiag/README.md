# WinUpdateDiag - Windows Update Diagnostic Utility

A comprehensive C# command-line utility for managing, diagnosing, and monitoring Windows Update functionality. Similar to the PSWindowsUpdate PowerShell module.

## Features

- **Configuration Display**: View Windows Update settings, WSUS configuration, and service status
- **Diagnostics**: Detect and report common Windows Update issues
- **Update Listing**: List available, pending, and installed updates
- **Update History**: View installation history with customizable entry count
- **Optional Updates**: Include or exclude optional updates in searches

## Requirements

- .NET Framework 4.7.2 or higher
- Windows operating system
- Administrator privileges (recommended for full functionality)
- COM reference to WUApiLib (Windows Update Agent API)

## Installation

1. Build the project in Visual Studio
2. Ensure the following COM reference is added:
   - WUApiLib (Windows Update Agent API Type Library)
   
   To add the reference in Visual Studio:
   - Right-click References → Add Reference
   - Go to COM tab → Type Libraries
   - Select "Windows Update Agent API Type Library" (wuapi.dll)

3. Add the following .NET references:
   - System.Management
   - System.ServiceProcess

## Usage

### Command-Line Options

```
WinUpdateDiag.exe [options]

Options:
  -h, --help              Show help message
  -c, --config            Display Windows Update configuration
  -d, --diagnose          Run diagnostics to detect issues
  -l, --list              List available updates
  -p, --pending           List pending (downloaded) updates
  -hi, --history [count]  Show update history (default: 20 entries)
  -o, --optional          Include optional updates when listing
  -v, --verbose           Show detailed information
  -a, --all               Run all checks and display all information
```

### Examples

#### Display Configuration
```cmd
WinUpdateDiag.exe --config
```
Shows:
- Auto Update settings
- WSUS server configuration
- Service status (wuauserv, BITS, cryptsvc, msiserver)
- Last update check times

#### Run Diagnostics
```cmd
WinUpdateDiag.exe --diagnose
```
Checks:
- Windows Update service status
- BITS service status
- Cryptographic services
- Windows Installer service
- Available disk space
- Pending reboot status
- Update database health
- Network connectivity to Microsoft servers
- System integrity

#### List Available Updates
```cmd
WinUpdateDiag.exe --list
```
Shows all available security and feature updates.

#### List Available Updates Including Optional
```cmd
WinUpdateDiag.exe --list --optional
```
Includes optional updates like drivers and feature packs.

#### List Pending Updates
```cmd
WinUpdateDiag.exe --pending
```
Shows updates that are downloaded but not yet installed.

#### View Update History
```cmd
WinUpdateDiag.exe --history 50
```
Displays the last 50 update installation entries.

#### Run All Checks
```cmd
WinUpdateDiag.exe --all
```
Performs all operations: configuration, diagnostics, list updates, pending updates, and history.

#### Verbose Output
```cmd
WinUpdateDiag.exe --list --verbose
```
Shows additional details including full descriptions and support URLs.

## Diagnostic Checks

The diagnostic feature checks:

1. **Windows Update Service (wuauserv)**: Ensures the core update service is running
2. **BITS Service**: Verifies Background Intelligent Transfer Service is operational
3. **Cryptographic Services**: Checks if crypto services required for update verification are running
4. **Windows Installer Service**: Validates installer service availability
5. **Disk Space**: Warns if free space is below 20GB, errors if below 10GB
6. **Pending Reboot**: Detects if a restart is required to complete previous updates
7. **Update Database**: Checks for database corruption or excessive size
8. **Network Connectivity**: Tests connection to Microsoft Update servers
9. **System Integrity**: Verifies critical Windows Update paths exist

## Configuration Information

The configuration display shows:

- Auto Update enabled/disabled status
- Auto Update behavior (notify, download, install automatically)
- WSUS server settings (if configured)
- Target group membership
- Scheduled installation day and time
- Service status for all update-related services
- Last successful download and search times

## Update Information Displayed

For each update, the tool shows:

- Title
- KB article numbers
- Download status
- Mandatory/optional classification
- Download size
- Reboot requirement
- Severity level
- Update categories

## Exit Codes

- `0`: Success
- Non-zero: Error occurred (check console output)

## Administrator Privileges

While the tool can run without administrator privileges, some features may be limited:

- Service status checks may fail
- Some registry keys may be inaccessible
- WMI queries might be restricted

It's recommended to run the tool as administrator for full functionality.

## Troubleshooting

### "Access Denied" Errors
Run the tool as administrator.

### "COM object creation failed"
Ensure Windows Update Agent API is properly registered:
```cmd
regsvr32 wuapi.dll
```

### No Updates Found
- Check network connectivity
- Verify Windows Update service is running
- Check if updates are being managed by WSUS
- Run diagnostics to identify issues

### Service Check Failures
Restart the required services:
```cmd
net start wuauserv
net start BITS
net start cryptsvc
```

## Similar PowerShell Functionality

This tool provides similar functionality to PSWindowsUpdate cmdlets:

| PSWindowsUpdate | WinUpdateDiag |
|----------------|---------------|
| Get-WindowsUpdate | --list |
| Get-WUSettings | --config |
| Get-WUHistory | --history |
| - | --diagnose |

## License

This is a diagnostic utility tool. Use at your own risk.

## Contributing

To extend functionality:
1. Add new diagnostic checks in `WindowsUpdateDiagnostics.cs`
2. Add new configuration options in `WindowsUpdateConfiguration.cs`
3. Extend command-line options in `CommandLineOptions.cs`
4. Add new update operations in `WindowsUpdateManager.cs`
