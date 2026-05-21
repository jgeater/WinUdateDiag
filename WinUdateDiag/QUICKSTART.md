# Quick Start Guide

## ⚠️ IMPORTANT: Add References First

The project **will not build** until you add the required references. Follow these steps:

## Step 1: Add .NET References

1. In **Solution Explorer**, right-click on **References**
2. Click **Add Reference...**
3. In the Reference Manager window, go to **Assemblies** → **Framework**
4. Check the boxes for:
   - ☑ **System.Management**
   - ☑ **System.ServiceProcess**
5. Click **OK**

## Step 2: Add COM Reference (Windows Update API)

1. In **Solution Explorer**, right-click on **References**
2. Click **Add Reference...**
3. In the Reference Manager window, go to the **COM** tab
4. Click on **Type Libraries**
5. Scroll down and find **"Windows Update Agent API Type Library"**
6. Check the box next to it
7. Click **OK**

**If you don't see it in the list:**
   - Click the **Browse** button at the bottom
   - Navigate to: `C:\Windows\System32\`
   - Find and select: `wuapi.dll`
   - Click **Add**, then **OK**

## Step 3: Verify References

In Solution Explorer, expand the **References** node. You should now see:
- ✓ System.Management
- ✓ System.ServiceProcess
- ✓ WUApiLib (or Interop.WUApiLib)

## Step 4: Build the Project

1. Press **Ctrl+Shift+B** or go to **Build** → **Build Solution**
2. Check the Output window - you should see "Build succeeded"

## Step 5: Run the Application

### Method 1: Run from Visual Studio
1. Press **F5** or click the **Start** button
2. By default it will show the help screen

### Method 2: Add Command-Line Arguments
1. Right-click the project in Solution Explorer
2. Select **Properties**
3. Go to the **Debug** tab
4. In **Command line arguments**, enter: `--all`
5. Press **F5** to run

### Method 3: Run from Command Line
1. Build the project
2. Open **Command Prompt** or **PowerShell** as **Administrator**
3. Navigate to: `bin\Debug\` or `bin\Release\`
4. Run: `WinUdateDiag.exe --help`

## Quick Test

Once built, test with:
```
WinUdateDiag.exe --config
```

This should display your Windows Update configuration.

## Common Issues

### "The type or namespace name 'WUApiLib' could not be found"
**Solution**: You haven't added the COM reference yet. Go back to Step 2.

### "The type or namespace name 'ServiceProcess' does not exist"
**Solution**: You haven't added System.ServiceProcess. Go back to Step 1.

### "Access Denied" when running
**Solution**: Run Visual Studio or the command prompt as Administrator.

### Build succeeded but nothing happens when running
**Solution**: The tool requires command-line arguments. Use `--help` to see options, or use `--all` to run everything.

## Example Commands

After building successfully, try these:

```powershell
# Show help
WinUdateDiag.exe --help

# Show configuration only
WinUdateDiag.exe --config

# Run diagnostics only
WinUdateDiag.exe --diagnose

# List available updates
WinUdateDiag.exe --list

# List updates including optional ones
WinUdateDiag.exe --list --optional

# Show update history (last 20)
WinUdateDiag.exe --history

# Show last 50 history entries
WinUdateDiag.exe --history 50

# Do everything
WinUdateDiag.exe --all

# Verbose output with all info
WinUdateDiag.exe --all --verbose
```

## Next Steps

- See **README.md** for complete documentation
- See **SETUP.md** for detailed setup instructions and troubleshooting
- Run with **--all** flag to see all features in action

## Features at a Glance

✓ Display Windows Update configuration  
✓ Show WSUS settings  
✓ Check service status  
✓ Diagnose common issues  
✓ List available updates  
✓ Show pending updates  
✓ View update history  
✓ Include optional updates  
✓ Network connectivity tests  
✓ Disk space checks  
✓ Pending reboot detection  

**Similar to PSWindowsUpdate PowerShell module, but as a standalone C# utility!**
