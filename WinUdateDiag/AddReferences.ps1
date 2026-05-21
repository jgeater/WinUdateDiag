# PowerShell script to add missing references to the project file

$projectFile = "WinUdateDiag.csproj"

Write-Host "Adding missing references to $projectFile..." -ForegroundColor Cyan

# Load the project file as XML
[xml]$proj = Get-Content $projectFile

# Create namespace manager for XML
$ns = New-Object System.Xml.XmlNamespaceManager($proj.NameTable)
$ns.AddNamespace("ms", "http://schemas.microsoft.com/developer/msbuild/2003")

# Find the ItemGroup with References
$referenceItemGroup = $proj.SelectSingleNode("//ms:ItemGroup[ms:Reference[@Include='System']]", $ns)

if ($referenceItemGroup -eq $null) {
    Write-Host "ERROR: Could not find Reference ItemGroup" -ForegroundColor Red
    exit 1
}

# Check if System.Management already exists
$existingMgmt = $referenceItemGroup.SelectSingleNode("ms:Reference[@Include='System.Management']", $ns)
if ($existingMgmt -eq $null) {
    # Add System.Management reference
    $mgmtRef = $proj.CreateElement("Reference", "http://schemas.microsoft.com/developer/msbuild/2003")
    $mgmtRef.SetAttribute("Include", "System.Management")
    $referenceItemGroup.AppendChild($mgmtRef) | Out-Null
    Write-Host "  Added: System.Management" -ForegroundColor Green
} else {
    Write-Host "  Already exists: System.Management" -ForegroundColor Yellow
}

# Check if System.ServiceProcess already exists
$existingSvc = $referenceItemGroup.SelectSingleNode("ms:Reference[@Include='System.ServiceProcess']", $ns)
if ($existingSvc -eq $null) {
    # Add System.ServiceProcess reference
    $svcRef = $proj.CreateElement("Reference", "http://schemas.microsoft.com/developer/msbuild/2003")
    $svcRef.SetAttribute("Include", "System.ServiceProcess")
    $referenceItemGroup.AppendChild($svcRef) | Out-Null
    Write-Host "  Added: System.ServiceProcess" -ForegroundColor Green
} else {
    Write-Host "  Already exists: System.ServiceProcess" -ForegroundColor Yellow
}

# Check if COM ItemGroup already exists
$comItemGroup = $proj.SelectSingleNode("//ms:ItemGroup[ms:COMReference]", $ns)

if ($comItemGroup -eq $null) {
    # Create new ItemGroup for COM references
    $comItemGroup = $proj.CreateElement("ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003")
    
    # Create COMReference element
    $comRef = $proj.CreateElement("COMReference", "http://schemas.microsoft.com/developer/msbuild/2003")
    $comRef.SetAttribute("Include", "WUApiLib")
    
    # Add child elements
    $guid = $proj.CreateElement("Guid", "http://schemas.microsoft.com/developer/msbuild/2003")
    $guid.InnerText = "{B596CC9F-56E5-419E-A622-E01BB457431E}"
    $comRef.AppendChild($guid) | Out-Null
    
    $verMajor = $proj.CreateElement("VersionMajor", "http://schemas.microsoft.com/developer/msbuild/2003")
    $verMajor.InnerText = "2"
    $comRef.AppendChild($verMajor) | Out-Null
    
    $verMinor = $proj.CreateElement("VersionMinor", "http://schemas.microsoft.com/developer/msbuild/2003")
    $verMinor.InnerText = "0"
    $comRef.AppendChild($verMinor) | Out-Null
    
    $lcid = $proj.CreateElement("Lcid", "http://schemas.microsoft.com/developer/msbuild/2003")
    $lcid.InnerText = "0"
    $comRef.AppendChild($lcid) | Out-Null
    
    $wrapper = $proj.CreateElement("WrapperTool", "http://schemas.microsoft.com/developer/msbuild/2003")
    $wrapper.InnerText = "tlbimp"
    $comRef.AppendChild($wrapper) | Out-Null
    
    $isolated = $proj.CreateElement("Isolated", "http://schemas.microsoft.com/developer/msbuild/2003")
    $isolated.InnerText = "False"
    $comRef.AppendChild($isolated) | Out-Null
    
    $embed = $proj.CreateElement("EmbedInteropTypes", "http://schemas.microsoft.com/developer/msbuild/2003")
    $embed.InnerText = "True"
    $comRef.AppendChild($embed) | Out-Null
    
    $comItemGroup.AppendChild($comRef) | Out-Null
    
    # Add the COM ItemGroup after the Reference ItemGroup
    $referenceItemGroup.ParentNode.InsertAfter($comItemGroup, $referenceItemGroup) | Out-Null
    Write-Host "  Added: WUApiLib (COM Reference)" -ForegroundColor Green
} else {
    Write-Host "  Already exists: COM References" -ForegroundColor Yellow
}

# Save the modified project file
$proj.Save((Resolve-Path $projectFile).Path)

Write-Host "`nReferences added successfully!" -ForegroundColor Green
Write-Host "Please reload the solution in Visual Studio." -ForegroundColor Cyan
