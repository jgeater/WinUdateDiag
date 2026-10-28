# Git Commit Scheduler - Setup Complete

## What Was Created

### Scripts
- **commit-and-push.ps1** - PowerShell script that:
  - Checks for uncommitted changes
  - Stages all changes with `git add -A`
  - Creates a detailed commit message
  - Commits changes
  - Pushes to origin/master
  - Logs all operations with timestamps

- **commit-and-push.bat** - Batch wrapper for easy execution

### Documentation
- **TASK_SCHEDULER_SETUP.md** - Complete setup guide with all options
- **QUICK_START.md** - Fast 2-minute setup guide
- **README_SCHEDULER.md** - This file

## Ready to Use

The scripts are ready to be scheduled in Windows Task Scheduler. They will automatically:

✅ Check for changes before committing
✅ Stage all modified or new files
✅ Create a meaningful commit message
✅ Push to GitHub
✅ Log all operations to `commit-push.log`
✅ Handle errors gracefully

## Setup Instructions (Choose One)

### Option A: Quick Setup (Recommended for First-Time)
1. Follow steps in `QUICK_START.md`
2. Takes ~2 minutes
3. Works for basic scheduling

### Option B: Advanced Setup
1. Follow steps in `TASK_SCHEDULER_SETUP.md`
2. More configuration options
3. Better for advanced scenarios (running when not logged in, retry logic, etc.)

### Option C: PowerShell Command-Line
Run this in elevated PowerShell:
```powershell
$taskName = "WinUdateDiag Git Commit - 9pm Daily"
$taskPath = "C:\Users\jgeat\source\repos\WinUdateDiag\commit-and-push.ps1"
$scriptAction = New-ScheduledTaskAction `
	-Execute "powershell.exe" `
	-Argument "-ExecutionPolicy Bypass -File `"$taskPath`""
$trigger = New-ScheduledTaskTrigger `
	-Daily `
	-At 9:00PM `
	-TimeZone "Eastern Standard Time"
Register-ScheduledTask `
	-TaskName $taskName `
	-Action $scriptAction `
	-Trigger $trigger `
	-RunLevel Highest
```

## Next Steps

1. **Test the script manually first:**
   ```powershell
   cd C:\Users\jgeat\source\repos\WinUdateDiag
   .\commit-and-push.ps1
   ```

2. **Follow the setup guide** (QUICK_START.md or TASK_SCHEDULER_SETUP.md)

3. **Verify the task runs:**
   - Open Task Scheduler
   - Right-click the task
   - Select "Run"
   - Check the log file

## Commit Message

The script creates commits with this format:

```
Update service status check logic and logging configuration

Service Status Changes:
- Fixed service status matrix to properly evaluate startup types
- Enhanced service check with better error handling

Logging Configuration Updates:
- Modified PKGLOG handling to only use if folder exists
- Removed automatic directory creation

Documentation Updates:
- Removed C:\PKGLOG references from README.md
- Updated DIAGNOSTIC_STATUS_REFERENCE.md with new service matrix
```

## Log Files

Every execution creates an entry in: `commit-push.log`

Contains:
- Timestamp of execution
- All git operations performed
- Success/warning/error messages
- Commit hash and push status

View latest logs:
```powershell
Get-Content C:\Users\jgeat\source\repos\WinUdateDiag\commit-push.log -Tail 50
```

## When It Runs

After setting up Task Scheduler, the script will automatically:
- Run at **9:00 PM Eastern Time** every day
- Check for changes
- Commit if there are changes
- Do nothing if there are no changes
- Log all operations

## Manual Execution

You can also run the script anytime manually:

**Via batch file:**
```batch
C:\Users\jgeat\source\repos\WinUdateDiag\commit-and-push.bat
```

**Via PowerShell:**
```powershell
C:\Users\jgeat\source\repos\WinUdateDiag\commit-and-push.ps1
```

**Via cmd:**
```cmd
powershell -ExecutionPolicy Bypass -File "C:\Users\jgeat\source\repos\WinUdateDiag\commit-and-push.ps1"
```

## Troubleshooting

### Script doesn't run in Task Scheduler
- Ensure "Run with highest privileges" is checked
- Verify PowerShell execution policy allows scripts
- Check Task Scheduler History for error details

### Git operations fail
- Test git manually:
  ```bash
  cd C:\Users\jgeat\source\repos\WinUdateDiag
  git status
  ```
- Ensure git is configured:
  ```bash
  git config --global user.email "your.email@example.com"
  git config --global user.name "Your Name"
  ```

### No commits are being pushed
- The script silently exits if there are no changes (this is normal)
- Check the log file to see what happened
- Run manually to verify changes exist

## Support

For detailed troubleshooting and advanced options, see:
- `TASK_SCHEDULER_SETUP.md` - Complete guide with all details
- `QUICK_START.md` - Simplified setup guide
- `commit-push.log` - Execution logs

## Ready!

You're all set! Choose your setup method from the options above and follow the steps. The script will handle your git commits automatically every night at 9pm Eastern time. 🚀
