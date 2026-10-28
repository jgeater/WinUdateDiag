using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using Microsoft.Win32;

namespace WinUdateDiag
{
    /// <summary>
    /// Diagnoses common Windows Update issues
    /// </summary>
    public class WindowsUpdateDiagnostics
    {
        private List<DiagnosticResult> _results;

        public List<DiagnosticResult> Results => _results;

        public WindowsUpdateDiagnostics()
        {
            _results = new List<DiagnosticResult>();
        }

        public void RunDiagnostics()
        {
            Console.WriteLine("\n=== Running Windows Update Diagnostics ===\n");

            CheckWindowsUpdateService();
            CheckBITSService();
            CheckCryptographicService();
            CheckInstallerService();
            CheckDiskSpace();
            CheckPendingReboot();
            CheckUpdateDatabase();
            CheckLastScanStatus();
            CheckNetworkConnectivity();
            CheckSystemIntegrity();
            CheckFeatureUpdateBlocks();

            DisplayResults();
        }

        private void CheckWindowsUpdateService()
        {
            CheckService("wuauserv", "Windows Update Service");
        }

        private void CheckBITSService()
        {
            CheckService("BITS", "Background Intelligent Transfer Service");
        }

        private void CheckCryptographicService()
        {
            CheckService("cryptsvc", "Cryptographic Services");
        }

        private void CheckInstallerService()
        {
            CheckService("msiserver", "Windows Installer Service");
        }

        private void CheckService(string serviceName, string displayName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    sc.Refresh();

                    // Check if service exists and get its start mode
                    ServiceStartMode startMode = GetServiceStartMode(serviceName);

                    // Status matrix:
                    // Manual (running or not) = PASS (user-controlled startup)
                    // Automatic + Running = PASS (working as expected)
                    // Automatic + Not Running = ERROR (should be running)
                    // Disabled (running or not) = ERROR (must be enabled)

                    if (startMode == ServiceStartMode.Manual)
                    {
                        // Manual services are PASS whether running or not
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = displayName,
                            Status = DiagnosticStatus.Pass,
                            Message = $"{displayName} startup type is Manual ({(sc.Status == ServiceControllerStatus.Running ? "currently running" : "not running")}) - this is acceptable"
                        });
                    }
                    else if (startMode == ServiceStartMode.Disabled)
                    {
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = displayName,
                            Status = DiagnosticStatus.Error,
                            Message = $"{displayName} is disabled",
                            Recommendation = $"Enable the service using: sc config {serviceName} start=auto"
                        });
                    }
                    else if (startMode == ServiceStartMode.Automatic || startMode == ServiceStartMode.Boot || startMode == ServiceStartMode.System)
                    {
                        // Automatic startup types must be running
                        if (sc.Status == ServiceControllerStatus.Running)
                        {
                            _results.Add(new DiagnosticResult
                            {
                                CheckName = displayName,
                                Status = DiagnosticStatus.Pass,
                                Message = $"{displayName} is set to {startMode} and is running"
                            });
                        }
                        else
                        {
                            _results.Add(new DiagnosticResult
                            {
                                CheckName = displayName,
                                Status = DiagnosticStatus.Error,
                                Message = $"{displayName} is set to {startMode} but is not running (Status: {sc.Status})",
                                Recommendation = $"Start the service using: net start {serviceName}"
                            });
                        }
                    }
                    else
                    {
                        // Unknown or other start modes
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = displayName,
                            Status = DiagnosticStatus.Pass,
                            Message = $"{displayName} startup type is {startMode} ({(sc.Status == ServiceControllerStatus.Running ? "running" : "not running")})"
                        });
                    }
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
            {
                // Service doesn't exist
                _results.Add(new DiagnosticResult
                {
                    CheckName = displayName,
                    Status = DiagnosticStatus.Error,
                    Message = $"{displayName} service not found on this system"
                });
            }
            catch (Exception ex)
            {
                _results.Add(new DiagnosticResult
                {
                    CheckName = displayName,
                    Status = DiagnosticStatus.Error,
                    Message = $"Error checking {displayName}: {ex.Message}"
                });
            }
        }

        private ServiceStartMode GetServiceStartMode(string serviceName)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}"))
                {
                    if (key != null)
                    {
                        object startValue = key.GetValue("Start");
                        if (startValue != null && int.TryParse(startValue.ToString(), out int startMode))
                        {
                            return (ServiceStartMode)startMode;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If we can't read the registry, assume it's not disabled
            }

            return ServiceStartMode.Automatic;
        }

        private enum ServiceStartMode
        {
            Boot = 0,
            System = 1,
            Automatic = 2,
            Manual = 3,
            Disabled = 4
        }

        private void CheckDiskSpace()
        {
            try
            {
                DriveInfo systemDrive = new DriveInfo(Environment.GetEnvironmentVariable("SystemDrive") ?? "C:");
                long freeSpaceGB = systemDrive.AvailableFreeSpace / (1024 * 1024 * 1024);
                long totalSpaceGB = systemDrive.TotalSize / (1024 * 1024 * 1024);
                double freePercent = (double)systemDrive.AvailableFreeSpace / systemDrive.TotalSize * 100;

                if (freeSpaceGB < 10)
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Disk Space",
                        Status = DiagnosticStatus.Error,
                        Message = $"Low disk space: {freeSpaceGB} GB free ({freePercent:F1}%) out of {totalSpaceGB} GB",
                        Recommendation = "Free up at least 10 GB of disk space before installing updates"
                    });
                }
                else if (freeSpaceGB < 20)
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Disk Space",
                        Status = DiagnosticStatus.Warning,
                        Message = $"Disk space is getting low: {freeSpaceGB} GB free ({freePercent:F1}%) out of {totalSpaceGB} GB",
                        Recommendation = "Consider freeing up more disk space"
                    });
                }
                else
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Disk Space",
                        Status = DiagnosticStatus.Pass,
                        Message = $"Sufficient disk space: {freeSpaceGB} GB free ({freePercent:F1}%) out of {totalSpaceGB} GB"
                    });
                }
            }
            catch (Exception ex)
            {
                _results.Add(new DiagnosticResult
                {
                    CheckName = "Disk Space",
                    Status = DiagnosticStatus.Error,
                    Message = $"Error checking disk space: {ex.Message}"
                });
            }
        }

        private void CheckPendingReboot()
        {
            bool rebootPending = false;
            string reason = "";
            List<string> detectedKeys = new List<string>();

            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending"))
                {
                    if (key != null)
                    {
                        rebootPending = true;
                        reason = "Component Based Servicing";
                        detectedKeys.Add(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending");
                    }
                }

                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired"))
                {
                    if (key != null)
                    {
                        rebootPending = true;
                        reason = string.IsNullOrEmpty(reason) ? "Windows Update" : reason + ", Windows Update";
                        detectedKeys.Add(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired");
                    }
                }

                // Check for pending file rename operations (common indicator of pending reboot)
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager"))
                {
                    if (key != null)
                    {
                        var pendingFileRenameOperations = key.GetValue("PendingFileRenameOperations");
                        if (pendingFileRenameOperations != null)
                        {
                            rebootPending = true;
                            reason = string.IsNullOrEmpty(reason) ? "Pending file operations" : reason + ", Pending file operations";
                            detectedKeys.Add(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager [PendingFileRenameOperations]");
                        }
                    }
                }

                // Check for pending computer rename
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName"))
                {
                    if (key != null)
                    {
                        var computerName = key.GetValue("ComputerName") as string;

                        using (var pendingKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName"))
                        {
                            if (pendingKey != null)
                            {
                                var pendingComputerName = pendingKey.GetValue("ComputerName") as string;

                                if (!string.IsNullOrEmpty(computerName) && 
                                    !string.IsNullOrEmpty(pendingComputerName) && 
                                    !computerName.Equals(pendingComputerName, StringComparison.OrdinalIgnoreCase))
                                {
                                    rebootPending = true;
                                    reason = string.IsNullOrEmpty(reason) ? "Computer rename pending" : reason + ", Computer rename pending";
                                    detectedKeys.Add(@"HKLM\SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName");
                                }
                            }
                        }
                    }
                }

                if (rebootPending)
                {
                    string keyList = "\n  Registry keys detected:\n  - " + string.Join("\n  - ", detectedKeys);
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Pending Reboot",
                        Status = DiagnosticStatus.Warning,
                        Message = $"System reboot is pending due to: {reason}{keyList}",
                        Recommendation = "Restart the computer to complete previous updates"
                    });
                }
                else
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Pending Reboot",
                        Status = DiagnosticStatus.Pass,
                        Message = "No pending reboot required"
                    });
                }
            }
            catch (Exception ex)
            {
                _results.Add(new DiagnosticResult
                {
                    CheckName = "Pending Reboot",
                    Status = DiagnosticStatus.Error,
                    Message = $"Error checking pending reboot: {ex.Message}"
                });
            }
        }

        private void CheckUpdateDatabase()
        {
            try
            {
                string softwareDistribution = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "SoftwareDistribution");
                string dataStore = Path.Combine(softwareDistribution, "DataStore");

                if (Directory.Exists(dataStore))
                {
                    DirectoryInfo di = new DirectoryInfo(dataStore);
                    long sizeBytes = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
                    long sizeMB = sizeBytes / (1024 * 1024);

                    if (sizeMB > 1024)
                    {
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = "Update Database",
                            Status = DiagnosticStatus.Warning,
                            Message = $"Update database is large: {sizeMB} MB",
                            Recommendation = "Consider running Windows Update troubleshooter or resetting update components"
                        });
                    }
                    else
                    {
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = "Update Database",
                            Status = DiagnosticStatus.Pass,
                            Message = $"Update database size: {sizeMB} MB"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _results.Add(new DiagnosticResult
                {
                    CheckName = "Update Database",
                    Status = DiagnosticStatus.Warning,
                    Message = $"Could not check update database: {ex.Message}"
                });
            }
        }

        private void CheckLastScanStatus()
        {
            try
            {
                // Windows Update scan status is recorded in the System event log with provider "Microsoft-Windows-WindowsUpdateClient"
                // Event IDs: 19 = Installation Success, 20 = Installation Failure, 43/44 = Scan operations
                EventLog systemLog = new EventLog("System");

                DateTime earliestTime = DateTime.Now.AddDays(-7); // Check last 7 days
                EventLogEntryCollection entries = systemLog.Entries;

                // Find the most recent scan event (43 or 44)
                EventLogEntry lastScanEvent = null;
                // Find the most recent successful installation (Event ID 19)
                EventLogEntry lastSuccessfulInstall = null;
                // Find the most recent installation attempt (success or failure)
                EventLogEntry lastInstallAttempt = null;
                // Collect all successes and failures for history
                List<EventLogEntry> successEvents = new List<EventLogEntry>();
                List<EventLogEntry> failureEvents = new List<EventLogEntry>();

                foreach (EventLogEntry entry in entries)
                {
                    if (entry.Source == "Microsoft-Windows-WindowsUpdateClient" && entry.TimeGenerated >= earliestTime)
                    {
                        // Track most recent scan event
                        if ((entry.EventID == 43 || entry.EventID == 44) && 
                            (lastScanEvent == null || entry.TimeGenerated > lastScanEvent.TimeGenerated))
                        {
                            lastScanEvent = entry;
                        }

                        // Track most recent successful installation (excluding Store apps)
                        if (entry.EventID == 19 && !ShouldFilterOutUpdate(entry.Message))
                        {
                            successEvents.Add(entry);
                            if (lastSuccessfulInstall == null || entry.TimeGenerated > lastSuccessfulInstall.TimeGenerated)
                            {
                                lastSuccessfulInstall = entry;
                            }
                        }

                        // Collect failures (but filter out Store apps)
                        if (entry.EventID == 20 && !ShouldFilterOutUpdate(entry.Message))
                        {
                            failureEvents.Add(entry);
                        }

                        // Track most recent installation attempt (either success or failure)
                        if (entry.EventID == 19 || entry.EventID == 20)
                        {
                            if (lastInstallAttempt == null || entry.TimeGenerated > lastInstallAttempt.TimeGenerated)
                            {
                                lastInstallAttempt = entry;
                            }
                        }
                    }
                }

                // Report the most recent installation attempt
                if (lastInstallAttempt != null)
                {
                    if (lastInstallAttempt.EventID == 19)
                    {
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = "Last Windows Update Scan",
                            Status = DiagnosticStatus.Pass,
                            Message = $"Most recent update successful at {lastInstallAttempt.TimeGenerated:g}"
                        });
                    }
                    else if (lastInstallAttempt.EventID == 20)
                    {
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = "Last Windows Update Scan",
                            Status = DiagnosticStatus.Warning,
                            Message = $"Most recent update attempt failed at {lastInstallAttempt.TimeGenerated:g}",
                            Recommendation = "See failed update details below; check event log or run troubleshooter"
                        });
                    }
                }
                else if (lastScanEvent != null)
                {
                    // If no installation attempts but we have scans
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Last Windows Update Scan",
                        Status = DiagnosticStatus.Pass,
                        Message = $"Scan activity detected at {lastScanEvent.TimeGenerated:g} (no installations yet)"
                    });
                }
                else
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Last Windows Update Scan",
                        Status = DiagnosticStatus.Error,
                        Message = "No Windows Update activity found in event log (last 7 days)",
                        Recommendation = "System has not run Windows Update scans in 7+ days. Check Update settings, restart Windows Update service, or run manual scan."
                    });
                }

                // Check for failures in the past 24 hours - this is an ERROR condition worth noting
                DateTime last24Hours = DateTime.Now.AddHours(-24);
                var recentFailures24h = failureEvents
                    .Where(e => e.TimeGenerated >= last24Hours)
                    .OrderByDescending(e => e.TimeGenerated)
                    .ToList();

                // Deduplicate failures by update identifier (keep only most recent of each)
                var deduplicatedFailures24h = DeduplicateFailures(recentFailures24h);

                if (deduplicatedFailures24h.Count > 0)
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Failed Updates (Last 24 Hours)",
                        Status = DiagnosticStatus.Error,
                        Message = $"⚠️ {deduplicatedFailures24h.Count} unique update failure(s) in the last 24 hours",
                        Recommendation = "Review failed updates below and check event log for error codes"
                    });

                    foreach (EventLogEntry failureEvent in deduplicatedFailures24h.Take(3)) // Show up to 3 unique failures from last 24h
                    {
                        string failureDetails = ExtractUpdateDetailsFromEventLog(failureEvent.Message);
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = "Recent Failure (24h)",
                            Status = DiagnosticStatus.Error,
                            Message = $"{failureEvent.TimeGenerated:g}: {failureDetails}"
                        });
                    }
                }

                // Show recent successful installations (up to 3 most recent)
                var recentSuccesses = successEvents
                    .OrderByDescending(e => e.TimeGenerated)
                    .Take(3)
                    .ToList();

                if (recentSuccesses.Count > 0)
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Recent Successful Installations",
                        Status = DiagnosticStatus.Pass,
                        Message = $"Last {recentSuccesses.Count} successful installation(s):"
                    });

                    foreach (EventLogEntry successEvent in recentSuccesses)
                    {
                        string updateDetails = ExtractUpdateDetailsFromEventLog(successEvent.Message);
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = "Successful Update",
                            Status = DiagnosticStatus.Pass,
                            Message = $"{successEvent.TimeGenerated:g}: {updateDetails}"
                        });
                    }
                }

                // Check for no MS updates (KB updates) in past 60 days - this is an ERROR condition
                DateTime last60Days = DateTime.Now.AddDays(-60);
                var ms60DayUpdates = successEvents
                    .Where(e => e.TimeGenerated >= last60Days && ContainsKBNumber(e.Message))
                    .ToList();

                if (ms60DayUpdates.Count == 0)
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Microsoft Updates (60 Days)",
                        Status = DiagnosticStatus.Error,
                        Message = "No Microsoft updates with KB numbers installed in the past 60 days",
                        Recommendation = "System may not be receiving critical security updates. Check Windows Update settings and run updates manually if needed."
                    });
                }

                // Show recent failed installations (up to 5 most recent unique failures)
                if (failureEvents.Count > 0)
                {
                    var deduplicatedFailures = DeduplicateFailures(failureEvents)
                        .OrderByDescending(e => e.TimeGenerated)
                        .Take(5)
                        .ToList();

                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Recent Failed Installations",
                        Status = DiagnosticStatus.Warning,
                        Message = $"Found {failureEvents.Count} total failures, {deduplicatedFailures.Count} unique update(s) showing last {deduplicatedFailures.Count}:"
                    });

                    foreach (EventLogEntry failureEvent in deduplicatedFailures)
                    {
                        string failureDetails = ExtractUpdateDetailsFromEventLog(failureEvent.Message);
                        _results.Add(new DiagnosticResult
                        {
                            CheckName = "Failed Update",
                            Status = DiagnosticStatus.Warning,
                            Message = $"{failureEvent.TimeGenerated:g}: {failureDetails}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _results.Add(new DiagnosticResult
                {
                    CheckName = "Last Windows Update Scan",
                    Status = DiagnosticStatus.Error,
                    Message = $"Error checking scan status: {ex.Message}"
                });
            }
        }

        private List<EventLogEntry> DeduplicateFailures(List<EventLogEntry> failureEvents)
        {
            // Deduplicate by extracting update identifier from the event message
            // Keep only the most recent occurrence of each unique update ID
            Dictionary<string, EventLogEntry> deduplicatedDict = new Dictionary<string, EventLogEntry>();

            foreach (EventLogEntry entry in failureEvents.OrderByDescending(e => e.TimeGenerated))
            {
                string updateId = ExtractUpdateIdFromMessage(entry.Message);

                if (!string.IsNullOrEmpty(updateId) && !deduplicatedDict.ContainsKey(updateId))
                {
                    deduplicatedDict[updateId] = entry;
                }
                else if (string.IsNullOrEmpty(updateId))
                {
                    // If we can't extract an ID, use the message as the key
                    string messageKey = entry.Message.Substring(0, Math.Min(100, entry.Message.Length));
                    if (!deduplicatedDict.ContainsKey(messageKey))
                    {
                        deduplicatedDict[messageKey] = entry;
                    }
                }
            }

            return deduplicatedDict.Values.OrderByDescending(e => e.TimeGenerated).ToList();
        }

        private string ExtractUpdateIdFromMessage(string eventMessage)
        {
            // Try to extract the update identifier (usually a GUID or package ID)
            // Examples: 9MV0B5HZVk97, KB12345678, or other identifiers
            if (string.IsNullOrEmpty(eventMessage))
                return null;

            // Look for patterns like "9XXXXX..." (Store package IDs) or "KBXXXXXX" or other update IDs
            var updateIdMatch = System.Text.RegularExpressions.Regex.Match(
                eventMessage, 
                @"(?:9[A-Z0-9]{8,}|KB\d+|[A-Z0-9]{8,}(?:\-[A-Z0-9]+)*)"
            );

            if (updateIdMatch.Success)
            {
                return updateIdMatch.Value.ToLower();
            }

            return null;
        }

        private bool ShouldFilterOutUpdate(string eventMessage)
        {
            if (string.IsNullOrEmpty(eventMessage))
                return false;

            // Filter out all Windows Store app updates - they are not critical system updates
            // Store app package IDs follow the pattern: 9XXXXXXXXXXX (9 followed by alphanumeric)
            // These include apps like Zune, music, video, games, and other Microsoft Store apps
            // while keeping actual system updates (KB numbers, Windows components, drivers, etc.)

            // Check if message contains a Windows Store package ID pattern (9 followed by 8+ alphanumeric chars)
            if (System.Text.RegularExpressions.Regex.IsMatch(eventMessage, @"\b9[A-Z0-9]{8,}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }

        private bool ContainsKBNumber(string eventMessage)
        {
            if (string.IsNullOrEmpty(eventMessage))
                return false;

            // Check if message contains a KB number (KB followed by digits)
            // Format: KB123456 or KB-123456
            return System.Text.RegularExpressions.Regex.IsMatch(eventMessage, @"KB[0-9]{6,}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private string ExtractUpdateDetailsFromEventLog(string eventMessage)
        {
            if (string.IsNullOrEmpty(eventMessage))
                return "Unknown error";

            // Event messages contain details like:
            // "Installation Failure: Windows failed to install the following update with error 0x80073D02: 9WZDNCRFJ3P2-MICROSOFT.ZUNEVIDEO"
            // Extract the error code and update name if available

            if (eventMessage.Contains("error"))
            {
                // Try to extract error code (0x followed by hex digits)
                var match = System.Text.RegularExpressions.Regex.Match(eventMessage, @"0x[0-9A-Fa-f]+");
                if (match.Success)
                {
                    var errorCode = match.Value;

                    // Try to extract the update name (usually follows the error code)
                    var updateMatch = System.Text.RegularExpressions.Regex.Match(eventMessage, @":\s*(.+?)(?:\s*$|\s*-\s*)");
                    if (updateMatch.Success && updateMatch.Groups.Count > 1)
                    {
                        string updateName = updateMatch.Groups[1].Value.Trim();
                        if (!string.IsNullOrEmpty(updateName) && updateName != errorCode)
                            return $"{errorCode}: {updateName}";
                    }

                    return errorCode;
                }
            }

            // Return first 100 characters of message if we can't parse it
            return eventMessage.Length > 100 ? eventMessage.Substring(0, 100) + "..." : eventMessage;
        }

        private void CheckNetworkConnectivity()
        {
            try
            {
                // Try HTTP request first (more reliable than ping for Windows Update servers)
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using (var client = new System.Net.WebClient())
                {
                    client.Headers.Add("User-Agent", "WinUpdateDiag");
                    client.Proxy = System.Net.WebRequest.DefaultWebProxy;
                    client.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                    try
                    {
                        // Try to download a small file from Microsoft's update servers
                        client.DownloadData("http://www.msftconnecttest.com/connecttest.txt");
                        stopwatch.Stop();

                        _results.Add(new DiagnosticResult
                        {
                            CheckName = "Network Connectivity",
                            Status = DiagnosticStatus.Pass,
                            Message = $"Can reach Microsoft servers ({stopwatch.ElapsedMilliseconds}ms)"
                        });
                        return;
                    }
                    catch
                    {
                        // If HTTP fails, try ping to a reliable Microsoft server
                        stopwatch.Restart();
                        using (var ping = new System.Net.NetworkInformation.Ping())
                        {
                            var result = ping.Send("8.8.8.8", 5000); // Google DNS as fallback
                            stopwatch.Stop();

                            if (result.Status == System.Net.NetworkInformation.IPStatus.Success)
                            {
                                _results.Add(new DiagnosticResult
                                {
                                    CheckName = "Network Connectivity",
                                    Status = DiagnosticStatus.Warning,
                                    Message = $"Internet is reachable but cannot connect to Microsoft servers",
                                    Recommendation = "Check firewall settings and proxy configuration for Windows Update"
                                });
                            }
                            else
                            {
                                _results.Add(new DiagnosticResult
                                {
                                    CheckName = "Network Connectivity",
                                    Status = DiagnosticStatus.Error,
                                    Message = $"No network connectivity detected",
                                    Recommendation = "Check network connection and verify internet access"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _results.Add(new DiagnosticResult
                {
                    CheckName = "Network Connectivity",
                    Status = DiagnosticStatus.Warning,
                    Message = $"Network connectivity check failed: {ex.Message}",
                    Recommendation = "Verify internet connection and DNS settings"
                });
            }
        }

        private void CheckSystemIntegrity()
        {
            try
            {
                string systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
                string[] criticalPaths = {
                    Path.Combine(systemRoot, "System32"),
                    Path.Combine(systemRoot, "SoftwareDistribution"),
                    Path.Combine(systemRoot, "System32", "catroot2")
                };

                bool allPathsExist = criticalPaths.All(Directory.Exists);

                if (allPathsExist)
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "System Integrity",
                        Status = DiagnosticStatus.Pass,
                        Message = "Critical Windows Update paths exist"
                    });
                }
                else
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "System Integrity",
                        Status = DiagnosticStatus.Error,
                        Message = "One or more critical Windows Update paths are missing",
                        Recommendation = "Run System File Checker (sfc /scannow)"
                    });
                }
            }
            catch (Exception ex)
            {
                _results.Add(new DiagnosticResult
                {
                    CheckName = "System Integrity",
                    Status = DiagnosticStatus.Warning,
                    Message = $"System integrity check failed: {ex.Message}"
                });
            }
        }

        private void CheckFeatureUpdateBlocks()
        {
            try
            {
                var blockingSettings = new List<string>();

                // Check for feature update deferrals
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"))
                {
                    if (key != null)
                    {
                        var deferFeatureUpdates = key.GetValue("DeferFeatureUpdates");
                        if (deferFeatureUpdates != null && Convert.ToInt32(deferFeatureUpdates) == 1)
                        {
                            var deferDays = key.GetValue("DeferFeatureUpdatesPeriodInDays");
                            blockingSettings.Add($"Feature updates deferred by {deferDays ?? "unknown"} days");
                        }

                        var pauseFeatureUpdates = key.GetValue("PauseFeatureUpdates");
                        if (pauseFeatureUpdates != null && Convert.ToInt32(pauseFeatureUpdates) == 1)
                        {
                            blockingSettings.Add("Feature updates are paused");
                        }

                        var targetReleaseVersion = key.GetValue("TargetReleaseVersion");
                        if (targetReleaseVersion != null && Convert.ToInt32(targetReleaseVersion) == 1)
                        {
                            var targetVersion = key.GetValue("TargetReleaseVersionInfo");
                            blockingSettings.Add($"Feature updates limited to version: {targetVersion}");
                        }

                        var productVersion = key.GetValue("ProductVersion");
                        if (productVersion != null)
                        {
                            blockingSettings.Add($"Product version restricted to: {productVersion}");
                        }
                    }
                }

                // Check Windows Update for Business settings
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings"))
                {
                    if (key != null)
                    {
                        var pauseFeatureUpdatesStartTime = key.GetValue("PauseFeatureUpdatesStartTime");
                        var pauseFeatureUpdatesEndTime = key.GetValue("PauseFeatureUpdatesEndTime");

                        if (pauseFeatureUpdatesStartTime != null && pauseFeatureUpdatesEndTime != null)
                        {
                            blockingSettings.Add($"Feature updates paused until: {pauseFeatureUpdatesEndTime}");
                        }

                        var deferFeatureUpdatesPeriodInDays = key.GetValue("DeferFeatureUpdatesPeriodInDays");
                        if (deferFeatureUpdatesPeriodInDays != null)
                        {
                            blockingSettings.Add($"Feature updates deferred: {deferFeatureUpdatesPeriodInDays} days");
                        }
                    }
                }

                // Check for Windows 10/11 upgrade blocks
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"))
                {
                    if (key != null)
                    {
                        var disableOSUpgrade = key.GetValue("DisableOSUpgrade");
                        if (disableOSUpgrade != null && Convert.ToInt32(disableOSUpgrade) == 1)
                        {
                            blockingSettings.Add("OS upgrades are disabled");
                        }
                    }
                }

                if (blockingSettings.Count > 0)
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Feature Update Restrictions",
                        Status = DiagnosticStatus.Warning,
                        Message = $"Found {blockingSettings.Count} setting(s) that may block or limit feature updates:\n      " + string.Join("\n      ", blockingSettings),
                        Recommendation = "Review these settings if feature updates are needed"
                    });
                }
                else
                {
                    _results.Add(new DiagnosticResult
                    {
                        CheckName = "Feature Update Restrictions",
                        Status = DiagnosticStatus.Pass,
                        Message = "No feature update restrictions found"
                    });
                }
            }
            catch (Exception ex)
            {
                _results.Add(new DiagnosticResult
                {
                    CheckName = "Feature Update Restrictions",
                    Status = DiagnosticStatus.Warning,
                    Message = $"Could not check feature update restrictions: {ex.Message}"
                });
            }
        }

        private void DisplayResults()
        {
            Console.WriteLine("\n=== Diagnostic Results ===\n");

            int pass = _results.Count(r => r.Status == DiagnosticStatus.Pass);
            int warning = _results.Count(r => r.Status == DiagnosticStatus.Warning);
            int error = _results.Count(r => r.Status == DiagnosticStatus.Error);

            foreach (var result in _results)
            {
                string statusSymbol = result.Status == DiagnosticStatus.Pass ? "[✓]" :
                                     result.Status == DiagnosticStatus.Warning ? "[!]" : "[✗]";
                
                ConsoleColor color = result.Status == DiagnosticStatus.Pass ? ConsoleColor.Green :
                                   result.Status == DiagnosticStatus.Warning ? ConsoleColor.Yellow : ConsoleColor.Red;

                Console.ForegroundColor = color;
                Console.Write(statusSymbol);
                Console.ResetColor();
                Console.WriteLine($" {result.CheckName}: {result.Message}");

                if (!string.IsNullOrEmpty(result.Recommendation))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"    → {result.Recommendation}");
                    Console.ResetColor();
                }
            }

            Console.WriteLine($"\nSummary: {pass} passed, {warning} warnings, {error} errors");
        }
    }

    public enum DiagnosticStatus
    {
        Pass,
        Warning,
        Error
    }

    public class DiagnosticResult
    {
        public string CheckName { get; set; }
        public DiagnosticStatus Status { get; set; }
        public string Message { get; set; }
        public string Recommendation { get; set; }
    }
}
