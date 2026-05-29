using System;
using System.IO;
using System.Text;

namespace WinUdateDiag
{
    /// <summary>
    /// Custom TextWriter that writes to both console and file
    /// </summary>
    public class DualWriter : TextWriter
    {
        private readonly TextWriter _consoleWriter;
        private readonly StreamWriter _fileWriter;

        public DualWriter(TextWriter consoleWriter, string logFilePath)
        {
            _consoleWriter = consoleWriter;

            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Open file for writing (overwrite if exists)
                _fileWriter = new StreamWriter(logFilePath, false, Encoding.UTF8);
                _fileWriter.AutoFlush = true;

                // Write session header
                string header = $"{'=',60}\nWinUpdateDiag Session Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{'=',60}\n";
                _fileWriter.Write(header);
            }
            catch (Exception ex)
            {
                _consoleWriter.WriteLine($"\nWarning: Could not create log file at {logFilePath}");
                _consoleWriter.WriteLine($"Error: {ex.Message}");
                _consoleWriter.WriteLine("Continuing without file logging...\n");
            }
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            _consoleWriter.Write(value);
            _fileWriter?.Write(value);
        }

        public override void Write(string value)
        {
            _consoleWriter.Write(value);
            _fileWriter?.Write(value);
        }

        public override void WriteLine(string value)
        {
            _consoleWriter.WriteLine(value);
            _fileWriter?.WriteLine(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _fileWriter != null)
            {
                try
                {
                    string footer = $"\n{'=',60}\nSession Ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{'=',60}\n";
                    _fileWriter.Write(footer);
                    _fileWriter.Flush();
                    _fileWriter.Dispose();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Helper class to determine log file path
    /// </summary>
    public static class LogPathHelper
    {
        public static string DetermineLogPath()
        {
            // Check Intune Management Extension logs directory first
            string intuneLogsPath = @"C:\ProgramData\Microsoft\IntuneManagementExtension\Logs";
            if (Directory.Exists(intuneLogsPath))
            {
                string logPath = Path.Combine(intuneLogsPath, "WinUdateDiag.log");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Logging to: {logPath}\n");
                Console.ResetColor();
                return logPath;
            }

            // Check PKGLOG directory
            string pkgLogPath = @"C:\PKGLOG";
            if (Directory.Exists(pkgLogPath))
            {
                string logPath = Path.Combine(pkgLogPath, "WinUdateDiag.log");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Logging to: {logPath}\n");
                Console.ResetColor();
                return logPath;
            }

            // No valid directory found
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning: No valid log directory found.");
            Console.WriteLine("Checked paths:");
            Console.WriteLine("  - C:\\ProgramData\\Microsoft\\IntuneManagementExtension\\Logs");
            Console.WriteLine("  - C:\\PKGLOG");
            Console.WriteLine("Continuing without file logging...\n");
            Console.ResetColor();
            return null;
        }
    }
}
