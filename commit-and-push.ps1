# WinUdateDiag Git Commit and Push Script
# This script stages all changes, creates a commit, and pushes to GitHub
# Schedule with Windows Task Scheduler to run at specific times

param(
	[switch]$LogOnly = $false
)

# Configuration
$repoPath = "C:\Users\jgeat\source\repos\WinUdateDiag"
$logFile = Join-Path $repoPath "commit-push.log"
$commitTimestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

function Write-Log {
	param(
		[string]$Message,
		[ValidateSet("Info", "Success", "Warning", "Error")]
		[string]$Level = "Info"
	)

	$logMessage = "[$commitTimestamp] [$Level] $Message"

	# Write to console with color
	switch ($Level) {
		"Success" { Write-Host $logMessage -ForegroundColor Green }
		"Warning" { Write-Host $logMessage -ForegroundColor Yellow }
		"Error" { Write-Host $logMessage -ForegroundColor Red }
		default { Write-Host $logMessage }
	}

	# Append to log file
	Add-Content -Path $logFile -Value $logMessage
}

try {
	Write-Log "Starting git commit and push process"
	Write-Log "Repository: $repoPath"

	# Change to repo directory
	Push-Location $repoPath

	# Check git status
	Write-Log "Checking git status..."
	$status = & git status --porcelain

	if ([string]::IsNullOrWhiteSpace($status)) {
		Write-Log "No changes to commit. Working directory is clean." "Warning"
		Pop-Location
		exit 0
	}

	Write-Log "Found changes to commit:"
	$status | ForEach-Object { Write-Log "  $_" }

	# Stage all changes
	Write-Log "Staging all changes..."
	& git add -A

	if ($LASTEXITCODE -ne 0) {
		throw "Failed to stage changes (exit code: $LASTEXITCODE)"
	}

	Write-Log "Changes staged successfully" "Success"

	# Create comprehensive commit message
	$commitMessage = @"
Update service status check logic and logging configuration

Service Status Changes:
- Fixed service status matrix to properly evaluate startup types
  * Manual startup services now always PASS (user-controlled)
  * Automatic startup services PASS only if running
  * Disabled services always FAIL
  * Missing services now properly detected as FAIL

- Enhanced service check with better error handling
  * Specific exception handling for missing services
  * Clearer status messages indicating startup type
  * Proper priority ordering (Disabled > Automatic > Manual)

Logging Configuration Updates:
- Modified PKGLOG handling to only use if folder exists
  * Removed automatic directory creation
  * No longer creates C:\PKGLOG if it doesn't exist
  * Silently skips if directory unavailable

Documentation Updates:
- Removed C:\PKGLOG references from README.md
- Removed C:\PKGLOG references from documentation files
- Updated DIAGNOSTIC_STATUS_REFERENCE.md with new service matrix
- Updated README.md with comprehensive service status table

Files Modified:
- WindowsUpdateDiagnostics.cs: Service check logic update
- LogWriter.cs: PKGLOG conditional handling
- CommandLineOptions.cs: Help text updates
- README.md: Documentation cleanup
- DIAGNOSTIC_STATUS_REFERENCE.md: Service matrix documentation
"@

	# Commit changes
	Write-Log "Creating commit..."
	$commitMessage | & git commit -F -

	if ($LASTEXITCODE -ne 0) {
		throw "Failed to create commit (exit code: $LASTEXITCODE)"
	}

	Write-Log "Commit created successfully" "Success"

	# Get commit hash
	$commitHash = & git rev-parse HEAD
	Write-Log "Commit Hash: $commitHash"

	# Push to remote
	Write-Log "Pushing to origin/master..."
	& git push origin master

	if ($LASTEXITCODE -ne 0) {
		throw "Failed to push to remote (exit code: $LASTEXITCODE)"
	}

	Write-Log "Changes pushed to GitHub successfully" "Success"

	# Show final status
	Write-Log "Verifying push..."
	$pushStatus = & git log --oneline -1
	Write-Log "Latest commit: $pushStatus" "Success"

	Write-Log "Git commit and push completed successfully" "Success"
}
catch {
	Write-Log "ERROR: $_" "Error"
	Write-Log "Commit and push failed. Please check the repository status." "Error"
	Pop-Location
	exit 1
}
finally {
	Pop-Location
	Write-Log "Script execution completed"
	Write-Log "Log file: $logFile"
}
