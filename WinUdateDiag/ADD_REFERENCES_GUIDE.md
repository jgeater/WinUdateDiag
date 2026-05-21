# How to Add Missing References

You have **TWO OPTIONS** to add the missing references:

---

## Option 1: Automated Script (Recommended)

### Steps:
1. **CLOSE Visual Studio completely**
2. Open PowerShell or Command Prompt
3. Navigate to the WinUdateDiag folder
4. Run: `.\AddReferences.bat`
5. **Reopen Visual Studio**
6. **Reload the solution** when prompted
7. **Build** (Ctrl+Shift+B)

---

## Option 2: Manual Via Visual Studio (If script doesn't work)

### Part A: Add .NET References

1. In **Solution Explorer**, right-click **References**
2. Click **Add Reference...**
3. Go to **Assemblies** → **Framework**
4. Check these boxes:
   - ☑ **System.Management**
   - ☑ **System.ServiceProcess**
5. Click **OK**

### Part B: Add COM Reference

1. Right-click **References** again
2. Click **Add Reference...**
3. Go to **COM** tab
4. Click **Type Libraries**
5. Find and check: **Windows Update Agent API Type Library**
6. Click **OK**

**If not found:**
- Click **Browse** button
- Go to: `C:\Windows\System32\`
- Select: `wuapi.dll`
- Click **Add**

---

## Option 3: Manual XML Edit (Advanced)

If both above options fail:

1. **CLOSE Visual Studio**
2. Open `WinUdateDiag.csproj` in a text editor (Notepad++)
3. Find this section:
```xml
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Xml.Linq" />
    <Reference Include="System.Data.DataSetExtensions" />
    <Reference Include="Microsoft.CSharp" />
    <Reference Include="System.Data" />
    <Reference Include="System.Net.Http" />
    <Reference Include="System.Xml" />
  </ItemGroup>
```

4. **ADD** these two lines before the closing `</ItemGroup>`:
```xml
    <Reference Include="System.Management" />
    <Reference Include="System.ServiceProcess" />
```

5. **ADD** this entire new section AFTER the `</ItemGroup>` above:
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

6. **SAVE** the file
7. **Reopen** Visual Studio
8. **Build** the solution

---

## Verify References Were Added

After adding references, in Solution Explorer under **References**, you should see:
- ✓ System.Management
- ✓ System.ServiceProcess  
- ✓ Interop.WUApiLib (or WUApiLib)

If you see all three, you're good to build!

---

## Still Having Issues?

### Error: "wuapi.dll not found"
**Solution**: The COM reference couldn't be resolved. Try:
```powershell
regsvr32 C:\Windows\System32\wuapi.dll
```

### Error: "Access Denied" 
**Solution**: Run Visual Studio as Administrator

### Build still fails after adding references
**Solution**: 
1. Clean solution (Build → Clean Solution)
2. Close VS
3. Delete `bin` and `obj` folders
4. Reopen VS
5. Rebuild (Ctrl+Shift+B)
