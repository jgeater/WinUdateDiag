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
