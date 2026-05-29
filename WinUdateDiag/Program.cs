using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace WinUdateDiag
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("  WinUpdateDiag - Windows Update Diagnostic Tool");
            Console.WriteLine("=================================================\n");

            if (!IsAdministrator())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("WARNING: Not running as administrator.");
                Console.WriteLine("Some features may not work correctly.\n");
                Console.ResetColor();
            }

            var options = CommandLineOptions.Parse(args);

            if (options.ShowHelp)
            {
                CommandLineOptions.DisplayHelp();
                return;
            }

            try
            {
                if (options.GetConfig)
                {
                    var config = WindowsUpdateConfiguration.GetConfiguration();
                    config.Display();
                }

                if (options.RunDiagnostics)
                {
                    var diagnostics = new WindowsUpdateDiagnostics();
                    diagnostics.RunDiagnostics();
                }

                if (options.ListUpdates)
                {
                    Console.WriteLine("\n=== Available Updates ===\n");
                    var manager = new WindowsUpdateManager();
                    var updates = manager.GetAvailableUpdates(options.IncludeOptional);

                    if (updates.Count == 0)
                    {
                        Console.WriteLine("No updates available.");
                    }
                    else
                    {
                        for (int i = 0; i < updates.Count; i++)
                        {
                            Console.WriteLine($"\nUpdate {i + 1} of {updates.Count}:");
                            Console.WriteLine(updates[i].ToString());

                            if (options.Verbose && !string.IsNullOrEmpty(updates[i].Description))
                            {
                                Console.WriteLine($"  Description: {updates[i].Description}");
                            }

                            if (!string.IsNullOrEmpty(updates[i].SupportUrl))
                            {
                                Console.WriteLine($"  Support URL: {updates[i].SupportUrl}");
                            }
                        }
                    }
                }

                if (options.ListPending)
                {
                    Console.WriteLine("\n=== Pending Updates (Downloaded) ===\n");
                    var manager = new WindowsUpdateManager();
                    var pending = manager.GetPendingUpdates();

                    if (pending.Count == 0)
                    {
                        Console.WriteLine("No pending updates.");
                    }
                    else
                    {
                        for (int i = 0; i < pending.Count; i++)
                        {
                            Console.WriteLine($"\nUpdate {i + 1} of {pending.Count}:");
                            Console.WriteLine(pending[i].ToString());
                        }
                    }
                }

                if (options.ListApplicable)
                {
                    Console.WriteLine("\n=== Applicable Updates (Not Installed) ===\n");
                    var manager = new WindowsUpdateManager();
                    var applicable = manager.GetApplicableUpdates(options.IncludeOptional);

                    if (applicable.Count == 0)
                    {
                        Console.WriteLine("No applicable updates found.");
                    }
                    else
                    {
                        int downloaded = applicable.Count(u => u.IsDownloaded);
                        int notDownloaded = applicable.Count - downloaded;

                        Console.WriteLine($"Total: {applicable.Count} update(s) - {downloaded} downloaded, {notDownloaded} not downloaded\n");

                        for (int i = 0; i < applicable.Count; i++)
                        {
                            Console.WriteLine($"\nUpdate {i + 1} of {applicable.Count}:");
                            Console.WriteLine(applicable[i].ToString());

                            if (options.Verbose && !string.IsNullOrEmpty(applicable[i].Description))
                            {
                                Console.WriteLine($"  Description: {applicable[i].Description}");
                            }

                            if (!string.IsNullOrEmpty(applicable[i].SupportUrl))
                            {
                                Console.WriteLine($"  Support URL: {applicable[i].SupportUrl}");
                            }
                        }
                    }
                }

                if (options.ListDrivers)
                {
                    Console.WriteLine("\n=== Driver Updates Blocked by MDM Policy ===\n");
                    var manager = new WindowsUpdateManager();
                    var drivers = manager.GetBlockedDrivers();

                    if (drivers.Count == 0)
                    {
                        Console.WriteLine("No blocked driver updates found (or drivers are not excluded by policy).");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Found {drivers.Count} driver update(s) that are blocked by the ExcludeWUDriversInQualityUpdate policy.\n");
                        Console.WriteLine("Note: These drivers will not be automatically installed by Windows Update.");
                        Console.WriteLine("Contact your IT administrator if you need these drivers installed.\n");
                        Console.ResetColor();

                        for (int i = 0; i < drivers.Count; i++)
                        {
                            Console.WriteLine($"\nDriver Update {i + 1} of {drivers.Count}:");
                            Console.WriteLine(drivers[i].ToString());

                            if (options.Verbose && !string.IsNullOrEmpty(drivers[i].Description))
                            {
                                Console.WriteLine($"  Description: {drivers[i].Description}");
                            }

                            if (!string.IsNullOrEmpty(drivers[i].SupportUrl))
                            {
                                Console.WriteLine($"  Support URL: {drivers[i].SupportUrl}");
                            }
                        }
                    }
                }

                if (options.ShowHistory)
                {
                    Console.WriteLine($"\n=== Update History (Last {options.HistoryCount} entries) ===\n");
                    var manager = new WindowsUpdateManager();
                    var history = manager.GetUpdateHistory(options.HistoryCount);

                    if (history.Count == 0)
                    {
                        Console.WriteLine("No update history available.");
                    }
                    else
                    {
                        for (int i = 0; i < history.Count; i++)
                        {
                            Console.WriteLine($"\n{i + 1}. {history[i]}");

                            if (options.Verbose && !string.IsNullOrEmpty(history[i].Description))
                            {
                                Console.WriteLine($"   Description: {history[i].Description}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: {ex.Message}");

                if (options.Verbose)
                {
                    Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
                }

                Console.ResetColor();
            }

            Console.WriteLine("\n=================================================");
        }

        static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
