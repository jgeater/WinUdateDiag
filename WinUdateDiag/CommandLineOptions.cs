using System;

namespace WinUdateDiag
{
    /// <summary>
    /// Command-line arguments parser and handler
    /// </summary>
    public class CommandLineOptions
    {
        public bool ShowHelp { get; set; }
        public bool GetConfig { get; set; }
        public bool RunDiagnostics { get; set; }
        public bool ListUpdates { get; set; }
        public bool ListPending { get; set; }
        public bool ListApplicable { get; set; }
        public bool ListDrivers { get; set; }
        public bool ShowHistory { get; set; }
        public bool IncludeOptional { get; set; }
        public int HistoryCount { get; set; } = 20;
        public bool Verbose { get; set; }

        public static CommandLineOptions Parse(string[] args)
        {
            var options = new CommandLineOptions();

            if (args.Length == 0)
            {
                options.ShowHelp = true;
                return options;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();

                switch (arg)
                {
                    case "-h":
                    case "--help":
                    case "/?":
                        options.ShowHelp = true;
                        break;

                    case "-c":
                    case "--config":
                        options.GetConfig = true;
                        break;

                    case "-d":
                    case "--diagnose":
                        options.RunDiagnostics = true;
                        break;

                    case "-l":
                    case "--list":
                        options.ListUpdates = true;
                        break;

                    case "-p":
                    case "--pending":
                        options.ListPending = true;
                        break;

                    case "-ap":
                    case "--applicable":
                        options.ListApplicable = true;
                        break;

                    case "-dr":
                    case "--drivers":
                        options.ListDrivers = true;
                        break;

                    case "-hi":
                    case "--history":
                        options.ShowHistory = true;
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int count))
                        {
                            options.HistoryCount = count;
                            i++;
                        }
                        break;

                    case "-o":
                    case "--optional":
                        options.IncludeOptional = true;
                        break;

                    case "-v":
                    case "--verbose":
                        options.Verbose = true;
                        break;

                    case "-a":
                    case "--all":
                        options.GetConfig = true;
                        options.RunDiagnostics = true;
                        options.ListUpdates = true;
                        options.ListPending = true;
                        options.ListApplicable = true;
                        options.ShowHistory = true;
                        break;

                    default:
                        Console.WriteLine($"Unknown option: {args[i]}");
                        Console.WriteLine("Use --help for usage information");
                        break;
                }
            }

            return options;
        }

        public static void DisplayHelp()
        {
            Console.WriteLine(@"
WinUpdateDiag - Windows Update Diagnostic Utility
==================================================

Usage: WinUpdateDiag.exe [options]

Options:
  -h, --help              Show this help message
  -c, --config            Display Windows Update configuration
  -d, --diagnose          Run diagnostics to detect issues
  -l, --list              List available updates
  -p, --pending           List pending (downloaded) updates
  -ap, --applicable       List applicable updates (not installed)
  -dr, --drivers          List driver updates blocked by MDM policy
  -hi, --history [count]  Show update history (default: 20 entries)
  -o, --optional          Include optional updates when listing
  -v, --verbose           Show detailed information
  -a, --all               Run all checks and display all information

Examples:
  WinUpdateDiag.exe --config
      Display current Windows Update configuration

  WinUpdateDiag.exe --diagnose
      Run diagnostics to identify potential issues

  WinUpdateDiag.exe --list
      List all available updates

  WinUpdateDiag.exe --applicable
      List all applicable updates that are not yet installed

  WinUpdateDiag.exe --drivers
      List driver updates that are blocked by MDM policy

  WinUpdateDiag.exe --list --optional
      List all available updates including optional ones

  WinUpdateDiag.exe --pending
      List updates that are downloaded but not installed

  WinUpdateDiag.exe --history 50
      Show last 50 update history entries

  WinUpdateDiag.exe --all
      Display configuration, run diagnostics, and list all updates

Notes:
  - This tool requires administrator privileges for full functionality
  - Some operations may take time depending on network speed
  - Network connectivity is required to search for updates
");
        }
    }
}
