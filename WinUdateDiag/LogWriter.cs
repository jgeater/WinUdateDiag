using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WinUdateDiag
{
    /// <summary>
    /// Custom TextWriter that writes to both console and multiple files
    /// </summary>
    public class DualWriter : TextWriter
    {
        private readonly TextWriter _consoleWriter;
        private readonly List<StreamWriter> _fileWriters;

        public DualWriter(TextWriter consoleWriter, List<string> logFilePaths)
        {
            _consoleWriter = consoleWriter;
            _fileWriters = new List<StreamWriter>();

            foreach (string logFilePath in logFilePaths)
            {
                try
                {
                    // Ensure directory exists
                    string directory = Path.GetDirectoryName(logFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        _consoleWriter.WriteLine($"✓ Created directory: {directory}");
                    }

                    // Open file for writing (overwrite if exists)
                    var fileWriter = new StreamWriter(logFilePath, false, Encoding.UTF8);
                    fileWriter.AutoFlush = true;

                    // Write session header
                    string header = $"{'=',60}\nWinUpdateDiag Session Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{'=',60}\n";
                    fileWriter.Write(header);

                    _fileWriters.Add(fileWriter);
                    _consoleWriter.WriteLine($"✓ Logging to: {logFilePath}");
                }
                catch (Exception ex)
                {
                    _consoleWriter.WriteLine($"\nWarning: Could not create log file at {logFilePath}");
                    _consoleWriter.WriteLine($"Error: {ex.Message}");
                }
            }

            if (_fileWriters.Count == 0)
            {
                _consoleWriter.WriteLine("Continuing without file logging...\n");
            }
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            _consoleWriter.Write(value);
            foreach (var fileWriter in _fileWriters)
            {
                fileWriter?.Write(value);
            }
        }

        public override void Write(string value)
        {
            _consoleWriter.Write(value);
            foreach (var fileWriter in _fileWriters)
            {
                fileWriter?.Write(value);
            }
        }

        public override void WriteLine(string value)
        {
            _consoleWriter.WriteLine(value);
            foreach (var fileWriter in _fileWriters)
            {
                fileWriter?.WriteLine(value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _fileWriters.Count > 0)
            {
                string footer = $"\n{'=',60}\nSession Ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{'=',60}\n";

                foreach (var fileWriter in _fileWriters)
                {
                    try
                    {
                        fileWriter.Write(footer);
                        fileWriter.Flush();
                        fileWriter.Dispose();
                    }
                    catch
                    {
                        // Ignore errors during cleanup
                    }
                }

                _fileWriters.Clear();
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Helper class to determine log file paths
    /// </summary>
    public static class LogPathHelper
    {
        public static List<string> DetermineLogPaths()
        {
            var logPaths = new List<string>();

            // Check/Create Intune Management Extension logs directory
            string intuneLogsPath = @"C:\ProgramData\Microsoft\IntuneManagementExtension\Logs";
            if (Directory.Exists(intuneLogsPath))
            {
                string logPath = Path.Combine(intuneLogsPath, "WinUdateDiag.log");
                logPaths.Add(logPath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Logging to: {logPath}");
                Console.ResetColor();
            }

            // Always add PKGLOG directory (create if it doesn't exist)
            string pkgLogPath = @"C:\PKGLOG";
            try
            {
                if (!Directory.Exists(pkgLogPath))
                {
                    Directory.CreateDirectory(pkgLogPath);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Created directory: {pkgLogPath}");
                    Console.ResetColor();
                }

                string logPath = Path.Combine(pkgLogPath, "WinUdateDiag.log");
                logPaths.Add(logPath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Logging to: {logPath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Could not create/access {pkgLogPath}");
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }

            if (logPaths.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: No valid log directories available.");
                Console.WriteLine("Continuing without file logging...");
                Console.ResetColor();
            }

            Console.WriteLine();
            return logPaths;
        }
    }
}
