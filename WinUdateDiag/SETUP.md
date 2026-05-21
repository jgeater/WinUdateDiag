# Project Setup Instructions

## Required References

To complete the setup of WinUpdateDiag, you need to add the following references to the project:

### .NET Framework References

1. **System.Management**
   - Right-click on "References" in Solution Explorer
   - Select "Add Reference"
   - Go to "Assemblies" → "Framework"
   - Check "System.Management"
   - Click OK

2. **System.ServiceProcess**
   - Right-click on "References" in Solution Explorer
   - Select "Add Reference"
   - Go to "Assemblies" → "Framework"
   - Check "System.ServiceProcess"
   - Click OK

### COM Reference

3. **WUApiLib (Windows Update Agent API)**
   - Right-click on "References" in Solution Explorer
   - Select "Add Reference"
   - Go to the "COM" tab
   - Select "Type Libraries"
   - Find and check "Windows Update Agent API Type Library" (wuapi.dll)
   - Click OK
   
   **Note**: If you don't see this in the list, you can add it via the Browse button:
   - Navigate to `C:\Windows\System32\wuapi.dll`
   - Select it and click "Add"

### Manual Project File Edit (Alternative Method)

If you prefer to edit the .csproj file directly:

1. Close Visual Studio
2. Open `WinUdateDiag.csproj` in a text editor
3. Find the `<ItemGroup>` section with `<Reference Include="System" />`
4. Add the following references:
   ```xml
   <Reference Include="System.Management" />
   <Reference Include="System.ServiceProcess" />
   ```

5. Add a new `<ItemGroup>` section for COM references:
   ```xml
   <ItemGroup>
     <COMReference Include="WUApiLib">
       <Guid>{B596CC9F-56E5-419E-A622-E01BB457431E}</Guid>
       <VersionMajor>2</VersionMajor>
       <VersionMinor>0</VersionMinor>
       <Lcid>0</Lcid>
       <WrapperTool>tlbimp</WrapperTool>
       <Isolated>False</Isolated>
       <EmbedInteropTypes>True</EmbedInteropTypes>
     </COMReference>
   </ItemGroup>
   ```

6. Save and reopen Visual Studio

## Building the Project

After adding all references:

1. Build the solution (Ctrl+Shift+B)
2. Verify there are no compilation errors
3. The executable will be created in `bin\Debug\WinUdateDiag.exe` or `bin\Release\WinUdateDiag.exe`

## Running the Application

### From Visual Studio
1. Set command-line arguments in Project Properties → Debug → Command line arguments
2. Example: `--diagnose --list`
3. Press F5 to run

### From Command Line
1. Open Command Prompt or PowerShell as Administrator
2. Navigate to the bin\Debug or bin\Release folder
3. Run: `WinUdateDiag.exe --help`

## Troubleshooting

### "The type or namespace name 'WUApiLib' could not be found"
- Ensure the COM reference to WUApiLib is properly added
- Try cleaning and rebuilding the solution
- Verify that wuapi.dll exists in C:\Windows\System32\

### "Access to the path is denied" when adding references
- Make sure Visual Studio is running as Administrator
- Close and reopen Visual Studio

### Build fails with COM interop errors
- Clean the solution (Build → Clean Solution)
- Delete the obj and bin folders
- Rebuild the solution

## Files Added to Project

The following files have been created:
- `Program.cs` - Main entry point with command-line processing
- `WindowsUpdateManager.cs` - Update search and retrieval
- `WindowsUpdateConfiguration.cs` - Configuration reading
- `WindowsUpdateDiagnostics.cs` - System diagnostics
- `CommandLineOptions.cs` - Command-line argument parsing
- `README.md` - User documentation

All files should be automatically included in the project. If not visible in Solution Explorer, click "Show All Files" and include them in the project.
