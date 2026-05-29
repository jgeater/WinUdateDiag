# WinUpdateDiag - Windows Update Diagnostic Utility

A comprehensive C# command-line utility for managing, diagnosing, and monitoring Windows Update functionality. Similar to the PSWindowsUpdate PowerShell module.

## Features

- **Configuration Display**: View Windows Update settings, WSUS configuration, service status, and registry key inspection
- **Diagnostics**: Detect and report common Windows Update issues with detailed error handling
- **Update Listing**: List available, pending, applicable, and installed updates with improved error handling for common issues
- **Update History**: View installation history with customizable entry count
- **Optional Updates**: Include or exclude optional updates in searches
- **Registry Reporting**: See all registry keys checked and their values for troubleshooting
- **Enhanced Error Messages**: Specific guidance for common errors like 0x80240032
- **Network Testing**: Uses Microsoft's official connectivity test endpoints
- **Feature Update Blocking Detection**: Identifies policies that prevent feature updates
- **MDM Policy Reporting**: Shows all MDM-managed update settings for enterprise environments

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
  -ap, --applicable       List applicable updates (not installed)
  -dr, --drivers          List driver updates blocked by MDM policy
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
- Feature update restrictions (deferrals, pauses, version targeting)
- MDM policies (Intune, etc.) affecting Windows Update

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

#### List Applicable Updates
```cmd
WinUpdateDiag.exe --applicable
```
Shows all updates that are applicable to the system but not yet installed (includes both downloaded and not downloaded updates). Provides a summary showing how many are already downloaded vs. need to be downloaded.

#### List Applicable Updates Including Optional
```cmd
WinUpdateDiag.exe --applicable --optional
```
Includes optional updates (drivers, etc.) in the applicable updates list.

#### List Driver Updates Blocked by MDM Policy
```cmd
WinUpdateDiag.exe --drivers
```
Shows driver updates that are available but blocked by the `ExcludeWUDriversInQualityUpdate` MDM policy. This is useful for:
- Understanding why drivers aren't appearing in regular update searches
- Identifying what drivers would be available without the policy
- Troubleshooting driver update issues on enterprise-managed machines

**Note:** If the MDM policy is enforced at the Windows Update API level, this command may not be able to enumerate the drivers and will provide guidance on alternative methods.

#### View Update History
```cmd
WinUpdateDiag.exe --history 50
```
Displays the last 50 update installation entries.

#### Run All Checks
```cmd
WinUpdateDiag.exe --all
```
Performs all operations: configuration, diagnostics, list updates, pending updates, applicable updates, and history.

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
10. **Feature Update Restrictions**: Detects settings that block or limit feature update installation
11. **MDM Policies**: Lists all MDM (Mobile Device Management) policies affecting Windows Update

### Network Connectivity Check Details
- **Primary test**: HTTP request to `http://www.msftconnecttest.com/connecttest.txt`
- **Fallback test**: Ping to `8.8.8.8` to verify basic internet connectivity
- **Respects proxy settings**: Uses system proxy configuration and credentials
- **Result interpretation**:
  - ✓ **Pass**: Can reach Microsoft servers
  - ⚠️ **Warning**: Internet works but Microsoft servers unreachable (firewall/proxy issue)
  - ✗ **Error**: No network connectivity detected

### Feature Update Restrictions Check
Detects the following restrictions:
- **Feature update deferrals**: Days to defer feature updates
- **Paused feature updates**: Whether feature updates are currently paused
- **Target release version**: If updates are limited to a specific Windows version
- **Product version restrictions**: Restrictions on Windows product version
- **OS upgrade disabled**: Complete block on OS upgrades
- **Windows Update for Business**: Deferral and pause settings

**Registry Keys Checked:**
- `HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate`
  - DeferFeatureUpdates, DeferFeatureUpdatesPeriodInDays
  - PauseFeatureUpdates
  - TargetReleaseVersion, TargetReleaseVersionInfo
  - ProductVersion
  - DisableOSUpgrade
- `HKLM\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings`
  - PauseFeatureUpdatesStartTime, PauseFeatureUpdatesEndTime
  - DeferFeatureUpdatesPeriodInDays

### MDM Policy Check
Reports MDM-managed Windows Update settings including:
- **MDM Enrollment Status**: Shows if device is enrolled with an MDM provider (Intune, etc.)
- **Update Policies**: All MDM-controlled update policies from `PolicyManager\current\device\Update`
- **Active Hours**: MDM-configured active hours for update installation
- **Branch Readiness Level**: Semi-Annual Channel configuration
- **Update Service URL**: Custom update server URLs configured via MDM

**Registry Keys Checked:**
- `HKLM\SOFTWARE\Microsoft\PolicyManager\current\device\Update`
- `HKLM\SOFTWARE\Microsoft\Enrollments`

**Useful for:**
- Corporate/managed devices to see what policies are in effect
- Troubleshooting why updates aren't installing
- Understanding organizational update policies

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

### Understanding Update States

The tool provides three different views of updates:

1. **Available Updates** (`--list`): Shows updates that Windows Update has found and made available to your system
2. **Pending Updates** (`--pending`): Shows updates that have been downloaded but are waiting to be installed
3. **Applicable Updates** (`--applicable`): Shows ALL updates that are not yet installed, regardless of download status
   - This is the most comprehensive view
   - Includes both downloaded and not-yet-downloaded updates
   - Shows a summary: "X downloaded, Y not downloaded"
   - Useful for getting a complete picture of what updates your system needs

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

### Feature Updates Not Installing
Run diagnostics to check for blocks:
```cmd
WinUpdateDiag.exe --diagnose
```
Look for the "Feature Update Restrictions" section which will show:
- Feature update deferrals
- Paused updates
- Version targeting restrictions
- OS upgrade blocks

### MDM/Intune Managed Devices
If your device is managed by Intune or another MDM:
```cmd
WinUpdateDiag.exe --diagnose
```
Check the "MDM Policies" section to see:
- What update policies are enforced
- Active hours configuration
- Branch readiness settings
- Custom update server URLs

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

### Driver Updates Not Showing in --applicable
If driver updates aren't appearing in `--applicable` results, they may be blocked by MDM policy:
```cmd
WinUpdateDiag.exe --drivers
```
This will:
- Check if the `ExcludeWUDriversInQualityUpdate` MDM policy is active
- Attempt to enumerate driver updates that are being blocked
- Provide guidance if drivers cannot be enumerated due to API-level enforcement

**Common Causes:**
- Enterprise MDM policies (Intune, Autopatch, WSUS)
- Group Policy settings that exclude driver updates
- IT departments often exclude drivers to control deployment separately

**Solutions:**
- Contact your IT administrator to request driver installation
- Check Device Manager for devices with available updates
- Temporarily disable the policy (requires admin/MDM access)

## Similar PowerShell Functionality

This tool provides similar functionality to PSWindowsUpdate cmdlets:

| PSWindowsUpdate | WinUpdateDiag |
|----------------|---------------|
| Get-WindowsUpdate | --list or --applicable |
| Get-WUSettings | --config |
| Get-WUHistory | --history |
| - | --diagnose |
| - | --pending |
| - | --drivers |

**Note**: The `--applicable` option is most similar to PSWindowsUpdate's `Get-WindowsUpdate` behavior, showing all updates that can be installed.

## License

This is a diagnostic utility tool. Use at your own risk.

## Contributing

To extend functionality:
1. Add new diagnostic checks in `WindowsUpdateDiagnostics.cs`
2. Add new configuration options in `WindowsUpdateConfiguration.cs`
3. Extend command-line options in `CommandLineOptions.cs`
4. Add new update operations in `WindowsUpdateManager.cs`
