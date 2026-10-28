using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace WinUdateDiag
{
    /// <summary>
    /// Handles storage and retrieval of diagnostic results in the Windows registry
    /// </summary>
    public class RegistryStorage
    {
        private const string REGISTRY_PATH = @"SOFTWARE\WinUpdateDiag";
        private const string REGISTRY_HIVE = "HKEY_LOCAL_MACHINE";

        /// <summary>
        /// Check if the application is running with administrator privileges
        /// </summary>
        public static bool IsRunningAsAdmin()
        {
            try
            {
                // Try to open the HKLM registry key for writing - this will fail if not admin
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var testKey = key.CreateSubKey("SOFTWARE"))
                {
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Store diagnostic results in the registry
        /// </summary>
        public static bool StoreDiagnosticResults(List<DiagnosticResult> results)
        {
            if (!IsRunningAsAdmin())
            {
                Console.WriteLine("[!] Warning: Not running as administrator. Cannot store results in registry.");
                return false;
            }

            try
            {
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    // Delete the existing key to ensure fresh data
                    try
                    {
                        hklm.DeleteSubKeyTree(REGISTRY_PATH, false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[!] Warning: Could not delete existing registry key: {ex.Message}");
                    }

                    // Create a new key
                    using (var key = hklm.CreateSubKey(REGISTRY_PATH))
                    {
                        if (key == null)
                        {
                            Console.WriteLine("[!] Error: Could not create registry key.");
                            return false;
                        }

                        // Store timestamp
                        key.SetValue("LastUpdate", DateTime.Now.ToString("o"), RegistryValueKind.String);

                        // Track counters per status
                        int passIndex = 0;
                        int warningIndex = 0;
                        int errorIndex = 0;

                        // Group results by status first, then by CheckName for organized storage
                        var passingResults = results.Where(r => r.Status == DiagnosticStatus.Pass)
                            .GroupBy(r => r.CheckName).OrderBy(g => g.Key);
                        var warningResults = results.Where(r => r.Status == DiagnosticStatus.Warning)
                            .GroupBy(r => r.CheckName).OrderBy(g => g.Key);
                        var errorResults = results.Where(r => r.Status == DiagnosticStatus.Error)
                            .GroupBy(r => r.CheckName).OrderBy(g => g.Key);

                        // Store passing results
                        foreach (var group in passingResults)
                        {
                            foreach (var result in group)
                            {
                                string valueName = $"Pass_{passIndex:D4}";
                                string resultText = FormatResultForRegistry(result);

                                if (resultText.Length > 2000)
                                {
                                    resultText = resultText.Substring(0, 1997) + "...";
                                }

                                key.SetValue(valueName, resultText, RegistryValueKind.String);
                                passIndex++;
                            }
                        }

                        // Store warning results
                        foreach (var group in warningResults)
                        {
                            foreach (var result in group)
                            {
                                string valueName = $"Warning_{warningIndex:D4}";
                                string resultText = FormatResultForRegistry(result);

                                if (resultText.Length > 2000)
                                {
                                    resultText = resultText.Substring(0, 1997) + "...";
                                }

                                key.SetValue(valueName, resultText, RegistryValueKind.String);
                                warningIndex++;
                            }
                        }

                        // Store error results
                        foreach (var group in errorResults)
                        {
                            foreach (var result in group)
                            {
                                string valueName = $"Error_{errorIndex:D4}";
                                string resultText = FormatResultForRegistry(result);

                                if (resultText.Length > 2000)
                                {
                                    resultText = resultText.Substring(0, 1997) + "...";
                                }

                                key.SetValue(valueName, resultText, RegistryValueKind.String);
                                errorIndex++;
                            }
                        }

                        // Store summary
                        int passCount = results.Count(r => r.Status == DiagnosticStatus.Pass);
                        int warningCount = results.Count(r => r.Status == DiagnosticStatus.Warning);
                        int errorCount = results.Count(r => r.Status == DiagnosticStatus.Error);

                        key.SetValue("Summary_Total", results.Count, RegistryValueKind.DWord);
                        key.SetValue("Summary_Passed", passCount, RegistryValueKind.DWord);
                        key.SetValue("Summary_Warnings", warningCount, RegistryValueKind.DWord);
                        key.SetValue("Summary_Errors", errorCount, RegistryValueKind.DWord);

                        Console.WriteLine($"[+] Diagnostic results stored in registry: {REGISTRY_HIVE}\\{REGISTRY_PATH}");
                        Console.WriteLine($"    Total results: {results.Count} (Passed: {passCount}, Warnings: {warningCount}, Errors: {errorCount})");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error storing results in registry: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieve diagnostic results from the registry
        /// </summary>
        public static List<DiagnosticResult> RetrieveDiagnosticResults()
        {
            var results = new List<DiagnosticResult>();

            try
            {
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = hklm.OpenSubKey(REGISTRY_PATH))
                {
                    if (key == null)
                    {
                        return results;
                    }

                    // Get all value names and filter for status-based results (Pass_, Warning_, Error_)
                    var valueNames = key.GetValueNames()
                        .Where(v => v.StartsWith("Pass_") || v.StartsWith("Warning_") || v.StartsWith("Error_"))
                        .OrderBy(v => 
                        {
                            // Sort by status priority (Pass, Warning, Error) then by index
                            if (v.StartsWith("Pass_")) return 0;
                            if (v.StartsWith("Warning_")) return 1;
                            return 2;
                        })
                        .ThenBy(v => v)
                        .ToList();

                    foreach (var valueName in valueNames)
                    {
                        string resultText = key.GetValue(valueName) as string;
                        if (!string.IsNullOrEmpty(resultText))
                        {
                            var result = ParseResultFromRegistry(resultText);
                            if (result != null)
                            {
                                results.Add(result);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error retrieving results from registry: {ex.Message}");
            }

            return results;
        }

        private static string FormatResultForRegistry(DiagnosticResult result)
        {
            // Format: CheckName | Status | Message | Recommendation
            return $"{result.CheckName}|{result.Status}|{result.Message}|{result.Recommendation ?? ""}";
        }

        private static DiagnosticResult ParseResultFromRegistry(string resultText)
        {
            try
            {
                var parts = resultText.Split('|');
                if (parts.Length < 3)
                    return null;

                return new DiagnosticResult
                {
                    CheckName = parts[0],
                    Status = (DiagnosticStatus)Enum.Parse(typeof(DiagnosticStatus), parts[1]),
                    Message = parts[2],
                    Recommendation = parts.Length > 3 ? parts[3] : null
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
