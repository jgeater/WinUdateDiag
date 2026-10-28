# Quick Start: Scheduling Git Commits at 9pm Eastern

## Files Created
✅ `commit-and-push.ps1` - Main PowerShell script that commits and pushes changes
✅ `commit-and-push.bat` - Batch wrapper to run the PowerShell script
✅ `TASK_SCHEDULER_SETUP.md` - Detailed setup guide
✅ `QUICK_START.md` - This file

## Quick Setup (2 minutes)

### Step 1: Test the Script
```powershell
cd C:\Users\jgeat\source\repos\WinUdateDiag
.\commit-and-push.ps1
```

### Step 2: Open Task Scheduler
- Press `Win + R`, type `taskschd.msc`, press Enter

### Step 3: Create New Task
1. Right-click "Task Scheduler Library" → "Create Task..."
2. Name: `WinUdateDiag Git Commit - 9pm Daily`
3. Go to "Triggers" → "New..."
   - Select: "On a schedule" → "Daily" 
   - Start time: 9:00 PM (21:00)
   - Time zone: Eastern Time
4. Go to "Actions" → "New..."
   - Program: `powershell.exe`
   - Arguments: `-ExecutionPolicy Bypass -File "C:\Users\jgeat\source\repos\WinUdateDiag\commit-and-push.ps1"`
5. Click OK
6. Enter your Windows password if prompted

### Step 4: Test the Task
- Right-click the task → "Run" to test it immediately
- Check log at: `C:\Users\jgeat\source\repos\WinUdateDiag\commit-push.log`

## That's It!

Your commits will automatically run every night at 9pm Eastern time.

## Check Status
- View logs: `C:\Users\jgeat\source\repos\WinUdateDiag\commit-push.log`
- View GitHub: https://github.com/jgeater/WinUdateDiag

## Need More Help?
See `TASK_SCHEDULER_SETUP.md` for detailed instructions and troubleshooting.
