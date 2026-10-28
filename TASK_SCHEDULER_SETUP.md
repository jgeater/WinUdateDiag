# Task Scheduler Setup Guide for WinUdateDiag Git Commits

## Overview
This guide explains how to schedule the `commit-and-push.ps1` script to run at 9pm Eastern time using Windows Task Scheduler.

## Prerequisites
- Git must be configured with your credentials or SSH key
- PowerShell execution policy must allow script execution
- You should be able to manually run `commit-and-push.bat` successfully before scheduling

## Step 1: Test the Script Manually

Before scheduling, test that the script works:

```powershell
cd C:\Users\jgeat\source\repos\WinUdateDiag
.\commit-and-push.ps1
```

Expected output:
- Green "Success" messages for each step
- Confirmation that changes were pushed to GitHub
- Log file created at: `C:\Users\jgeat\source\repos\WinUdateDiag\commit-push.log`

## Step 2: Configure PowerShell Execution Policy

If you get an execution policy error, run this command in an elevated PowerShell:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## Step 3: Schedule with Windows Task Scheduler

### Method A: Using Task Scheduler GUI (Recommended)

1. **Open Task Scheduler**
   - Press `Win + R`, type `taskschd.msc`, press Enter
   - Or: Settings → System → Tools → Task Scheduler

2. **Create a New Task**
   - Right-click on "Task Scheduler Library" → "Create Task..."
   - Name: `WinUdateDiag Git Commit - 9pm Daily`
   - Description: `Automatically commits and pushes daily changes to GitHub`

3. **Configure General Settings**
   - ✓ Check "Run with highest privileges" (to allow access to git operations)
   - Select "Run whether user is logged in or not" (optional, but recommended)
   - Configure for: Windows 10 (or your version)

4. **Set Trigger (Schedule)**
   - Click "Triggers" tab → "New..."
   - Begin the task: On a schedule
   - Repeat every: 1 day
   - Repeat task every: 24 hours
   - Start time: 9:00 PM (21:00)
   - Time zone: Eastern Time
   - ✓ Enable the trigger

5. **Configure Action (What to Run)**
   - Click "Actions" tab → "New..."
   - Action: Start a program
   - Program/script: `powershell.exe`
   - Add arguments: `-ExecutionPolicy Bypass -File "C:\Users\jgeat\source\repos\WinUdateDiag\commit-and-push.ps1"`
   - Start in: `C:\Users\jgeat\source\repos\WinUdateDiag`

6. **Configure Conditions (Optional)**
   - Click "Conditions" tab
   - Uncheck "Start task only if the computer is on AC power" (if laptop)
   - Check "Wake the computer to run this task" (if desired)

7. **Configure Settings**
   - Click "Settings" tab
   - ✓ Allow task to be run on demand
   - ✓ Stop the task if it runs longer than: 1 hour
   - If the task fails, retry every: 5 minutes (up to 3 times)
   - If the task has not terminated by this time, force it to close

8. **Save**
   - Click OK
   - Enter your Windows password if prompted

### Method B: Using PowerShell (Advanced)

Run this command in an elevated PowerShell to create the task:

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
	-RunLevel Highest `
	-Description "Automatically commits and pushes WinUdateDiag changes to GitHub at 9pm Eastern"
```

## Step 4: Verify the Task

1. Open Task Scheduler again
2. Navigate to Task Scheduler Library
3. Find "WinUdateDiag Git Commit - 9pm Daily"
4. Right-click and select "Run" to test immediately
5. Check the log file at: `C:\Users\jgeat\source\repos\WinUdateDiag\commit-push.log`

## Troubleshooting

### Task runs but script fails
- Check the log file: `commit-push.log`
- Run the script manually to identify errors
- Ensure git credentials are configured
- Try running with `-RunLevel Highest` in Task Scheduler

### "Access Denied" errors
- Ensure Task Scheduler is running "with highest privileges"
- Make sure you have full access to the repository folder

### Git operations fail
- Test git commands manually:
  ```bash
  cd C:\Users\jgeat\source\repos\WinUdateDiag
  git status
  git log --oneline -1
  ```
- Ensure git is in your PATH
- Configure git credentials:
  ```bash
  git config --global user.email "your.email@example.com"
  git config --global user.name "Your Name"
  ```

### PowerShell script not executing
- Check execution policy:
  ```powershell
  Get-ExecutionPolicy
  ```
- Set to RemoteSigned if needed:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```

## Manual Commitment

If you need to commit changes outside of the scheduled time:

```batch
C:\Users\jgeat\source\repos\WinUdateDiag\commit-and-push.bat
```

Or directly:

```powershell
C:\Users\jgeat\source\repos\WinUdateDiag\commit-and-push.ps1
```

## Log File Location

Each execution creates/appends to:
- `C:\Users\jgeat\source\repos\WinUdateDiag\commit-push.log`

This log contains:
- Timestamp of execution
- Git status and changes
- Commit hash
- Push status
- Any errors encountered

## Monitoring Scheduled Task

To view task execution history:

1. Open Task Scheduler
2. Find "WinUdateDiag Git Commit - 9pm Daily"
3. Click "History" tab to see past runs
4. Check the log file for detailed output

## Disabling the Task

If you need to temporarily disable the task:

```powershell
Disable-ScheduledTask -TaskName "WinUdateDiag Git Commit - 9pm Daily"
```

To re-enable:

```powershell
Enable-ScheduledTask -TaskName "WinUdateDiag Git Commit - 9pm Daily"
```

To remove entirely:

```powershell
Unregister-ScheduledTask -TaskName "WinUdateDiag Git Commit - 9pm Daily" -Confirm:$false
```

## Notes

- The script will only commit if there are changes
- If there are no changes, the script exits cleanly with no errors
- All commits are logged with a timestamp
- Script has a 1-hour timeout to prevent hanging
- Failed commits will be retried up to 3 times (with 5-minute intervals)
