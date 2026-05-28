# WinUpdateDiag - Windows Update Diagnostic Utility

A comprehensive C# command-line utility for managing, diagnosing, and monitoring Windows Update functionality. Similar to the PSWindowsUpdate PowerShell module.

## Features

- **Configuration Display**: View Windows Update settings, WSUS configuration, service status, and registry key inspection
- **Diagnostics**: Detect and report common Windows Update issues with detailed error handling
- **Update Listing**: List available, pending, and installed updates with improved error handling for common issues
- **Update History**: View installation history with customizable entry count
- **Optional Updates**: Include or exclude optional updates in searches
- **Registry Reporting**: See all registry keys checked and their values for troubleshooting
- **Enhanced Error Messages**: Specific guidance for common errors like 0x80240032
- **Network Testing**: Uses Microsoft's official connectivity test endpoints

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
- Registry keys checked with their values (shows which keys exist and their current settings)

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
8. **Network Connectivity**: Tests HTTP connection to Microsoft servers (uses `msftconnecttest.com` - the same endpoint Windows uses)
9. **System Integrity**: Verifies critical Windows Update paths exist

### Network Connectivity Check Details
- **Primary test**: HTTP request to `http://www.msftconnecttest.com/connecttest.txt`
- **Fallback test**: Ping to `8.8.8.8` to verify basic internet connectivity
- **Respects proxy settings**: Uses system proxy configuration and credentials
- **Result interpretation**:
  - ✓ **Pass**: Can reach Microsoft servers
  - ⚠️ **Warning**: Internet works but Microsoft servers unreachable (firewall/proxy issue)
  - ✗ **Error**: No network connectivity detected

## Configuration Information

The configuration display shows:

- Auto Update enabled/disabled status
- Auto Update behavior (notify, download, install automatically)
- WSUS server settings (if configured)
- Target group membership
- Scheduled installation day and time
- Service status for all update-related services
- Last successful download and search times
- **Registry keys checked**: All registry keys examined with their existence status and values
  - `HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate` - Policy settings
  - `HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU` - Auto Update policies
  - `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update` - Auto Update settings
  - `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Download` - Download results
  - `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Search` - Search results

## Update Information Displayed

For each update, the tool shows:

- Title
- KB article numbers
- Download status
- Mandatory/optional classification
- Download size
- Reboot requirement (determined from InstallationBehavior.RebootBehavior)
- Severity level
- Update categories

### Technical Notes
- **Reboot detection**: Uses `InstallationBehavior.RebootBehavior` to determine if reboot is required
- **Error handling**: Provides specific messages for common COM errors (0x80240032, 0x80240002, 0x80240437)
- **Fallback search**: If advanced search criteria fail, falls back to simpler criteria

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

### Error 0x80240032 (WU_E_INVALID_CRITERIA)
This error indicates invalid search criteria or Windows Update service issues:
- **Restart Windows Update service**: `net stop wuauserv && net start wuauserv`
- **Check if system restart is needed**: Run `WinUpdateDiag.exe --diagnose`
- The tool now provides detailed guidance when this error occurs

### Network Connectivity Failures
The tool uses HTTP requests to `msftconnecttest.com` instead of ping:
- If this fails, it tests general internet connectivity
- Check firewall settings if Microsoft servers are unreachable
- Verify proxy configuration if in a corporate environment

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
