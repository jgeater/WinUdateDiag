using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;

namespace WinUdateDiag
{
    /// <summary>
    /// Retrieves Windows Update configuration settings
    /// </summary>
    public class WindowsUpdateConfiguration
    {
        public string AutoUpdateOption { get; set; }
        public bool AutoUpdateEnabled { get; set; }
        public int ScheduledInstallDay { get; set; }
        public int ScheduledInstallTime { get; set; }
        public bool UseWUServer { get; set; }
        public string WUServer { get; set; }
        public string WUStatusServer { get; set; }
        public string TargetGroup { get; set; }
        public bool NoAutoUpdate { get; set; }
        public bool ElevateNonAdmins { get; set; }
        public string LastSuccessTime { get; set; }
        public string LastSearchSuccessTime { get; set; }
        public List<string> ServiceStatus { get; set; }

        public static WindowsUpdateConfiguration GetConfiguration()
        {
            var config = new WindowsUpdateConfiguration
            {
                ServiceStatus = new List<string>()
            };

            try
            {
                config.GetRegistrySettings();
                config.GetServiceStatus();
                config.GetWUASettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting configuration: {ex.Message}");
            }

            return config;
        }

        private void GetRegistrySettings()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"))
                {
                    if (key != null)
                    {
                        WUServer = key.GetValue("WUServer") as string;
                        WUStatusServer = key.GetValue("WUStatusServer") as string;
                        TargetGroup = key.GetValue("TargetGroup") as string;
                        ElevateNonAdmins = Convert.ToBoolean(key.GetValue("ElevateNonAdmins", 0));
                    }
                }

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
                {
                    if (key != null)
                    {
                        var auOption = key.GetValue("AUOptions");
                        if (auOption != null)
                        {
                            AutoUpdateOption = GetAutoUpdateOptionText((int)auOption);
                        }

                        NoAutoUpdate = Convert.ToBoolean(key.GetValue("NoAutoUpdate", 0));
                        ScheduledInstallDay = Convert.ToInt32(key.GetValue("ScheduledInstallDay", 0));
                        ScheduledInstallTime = Convert.ToInt32(key.GetValue("ScheduledInstallTime", 0));
                        UseWUServer = Convert.ToBoolean(key.GetValue("UseWUServer", 0));
                    }
                }

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update"))
                {
                    if (key != null)
                    {
                        var enabled = key.GetValue("EnableFeaturedSoftware");
                        AutoUpdateEnabled = enabled != null && Convert.ToBoolean(enabled);
                    }
                }

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Download"))
                {
                    if (key != null)
                    {
                        LastSuccessTime = key.GetValue("LastSuccessTime") as string;
                    }
                }

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Search"))
                {
                    if (key != null)
                    {
                        LastSearchSuccessTime = key.GetValue("LastSuccessTime") as string;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading registry: {ex.Message}");
            }
        }

        private void GetServiceStatus()
        {
            try
            {
                string[] services = { "wuauserv", "BITS", "cryptsvc", "msiserver" };
                
                foreach (string serviceName in services)
                {
                    using (ManagementObject service = new ManagementObject($"Win32_Service.Name='{serviceName}'"))
                    {
                        service.Get();
                        string state = service["State"]?.ToString() ?? "Unknown";
                        string startMode = service["StartMode"]?.ToString() ?? "Unknown";
                        ServiceStatus.Add($"{serviceName}: {state} (StartMode: {startMode})");
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceStatus.Add($"Error getting service status: {ex.Message}");
            }
        }

        private void GetWUASettings()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_Service WHERE Name = 'wuauserv'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string state = obj["State"]?.ToString();
                        AutoUpdateEnabled = state == "Running";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting WUA settings: {ex.Message}");
            }
        }

        private string GetAutoUpdateOptionText(int option)
        {
            switch (option)
            {
                case 1: return "Disabled";
                case 2: return "Notify before download";
                case 3: return "Download but notify before install";
                case 4: return "Automatic download and install";
                case 5: return "Allow local admin to choose setting";
                default: return $"Unknown ({option})";
            }
        }

        public void Display()
        {
            Console.WriteLine("\n=== Windows Update Configuration ===");
            Console.WriteLine($"Auto Update Enabled: {AutoUpdateEnabled}");
            Console.WriteLine($"Auto Update Option: {AutoUpdateOption ?? "Not configured"}");
            Console.WriteLine($"No Auto Update: {NoAutoUpdate}");
            Console.WriteLine($"Use WSUS Server: {UseWUServer}");
            
            if (!string.IsNullOrEmpty(WUServer))
                Console.WriteLine($"WSUS Server: {WUServer}");
            
            if (!string.IsNullOrEmpty(WUStatusServer))
                Console.WriteLine($"WSUS Status Server: {WUStatusServer}");
            
            if (!string.IsNullOrEmpty(TargetGroup))
                Console.WriteLine($"Target Group: {TargetGroup}");
            
            if (ScheduledInstallDay > 0)
                Console.WriteLine($"Scheduled Install Day: {GetDayOfWeek(ScheduledInstallDay)}");
            
            if (ScheduledInstallTime > 0)
                Console.WriteLine($"Scheduled Install Time: {ScheduledInstallTime:D2}:00");
            
            Console.WriteLine($"Elevate Non-Admins: {ElevateNonAdmins}");
            
            if (!string.IsNullOrEmpty(LastSuccessTime))
                Console.WriteLine($"Last Download Success: {LastSuccessTime}");
            
            if (!string.IsNullOrEmpty(LastSearchSuccessTime))
                Console.WriteLine($"Last Search Success: {LastSearchSuccessTime}");
            
            Console.WriteLine("\n=== Service Status ===");
            foreach (var status in ServiceStatus)
            {
                Console.WriteLine(status);
            }
        }

        private string GetDayOfWeek(int day)
        {
            switch (day)
            {
                case 0: return "Every day";
                case 1: return "Sunday";
                case 2: return "Monday";
                case 3: return "Tuesday";
                case 4: return "Wednesday";
                case 5: return "Thursday";
                case 6: return "Friday";
                case 7: return "Saturday";
                default: return $"Unknown ({day})";
            }
        }
    }
}
