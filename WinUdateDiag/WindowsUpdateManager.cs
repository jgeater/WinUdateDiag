using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WUApiLib;

namespace WinUdateDiag
{
    /// <summary>
    /// Manages Windows Update operations including searching, configuration, and diagnostics
    /// </summary>
    public class WindowsUpdateManager
    {
        private readonly UpdateSession _updateSession;
        private readonly IUpdateSearcher _updateSearcher;

        public WindowsUpdateManager()
        {
            _updateSession = new UpdateSession();
            _updateSearcher = _updateSession.CreateUpdateSearcher();
        }

        /// <summary>
        /// Gets available updates from Windows Update
        /// </summary>
        public List<UpdateInfo> GetAvailableUpdates(bool includeOptional = false)
        {
            var updates = new List<UpdateInfo>();
            try
            {
                Console.WriteLine("Searching for updates...");

                // Simplified search criteria - more compatible across systems
                string searchCriteria = includeOptional 
                    ? "IsInstalled=0" 
                    : "IsInstalled=0 and Type='Software'";

                ISearchResult searchResult = _updateSearcher.Search(searchCriteria);

                Console.WriteLine($"Found {searchResult.Updates.Count} update(s)");

                foreach (IUpdate update in searchResult.Updates)
                {
                    updates.Add(new UpdateInfo
                    {
                        Title = update.Title,
                        Description = update.Description,
                        IsDownloaded = update.IsDownloaded,
                        IsMandatory = update.IsMandatory,
                        KBArticleIDs = GetKBArticles(update.KBArticleIDs),
                        MaxDownloadSize = update.MaxDownloadSize,
                        MinDownloadSize = update.MinDownloadSize,
                        RebootRequired = update.InstallationBehavior?.RebootBehavior != WUApiLib.InstallationRebootBehavior.irbNeverReboots,
                        SeverityLevel = update.MsrcSeverity,
                        UpdateID = update.Identity.UpdateID,
                        SupportUrl = update.SupportUrl,
                        Categories = GetCategories(update.Categories)
                    });
                }
            }
            catch (COMException ex)
            {
                HandleSearchError(ex, "available updates");
            }

            return updates;
        }

        /// <summary>
        /// Gets pending updates that are downloaded but not installed
        /// </summary>
        public List<UpdateInfo> GetPendingUpdates()
        {
            var updates = new List<UpdateInfo>();
            try
            {
                Console.WriteLine("Searching for pending updates...");

                // Try simple search first
                ISearchResult searchResult;
                try
                {
                    searchResult = _updateSearcher.Search("IsInstalled=0 and IsPresent=1");
                }
                catch (COMException)
                {
                    // Fallback to simpler criteria if IsPresent causes issues
                    searchResult = _updateSearcher.Search("IsInstalled=0");
                }

                int pendingCount = 0;
                foreach (IUpdate update in searchResult.Updates)
                {
                    // Filter for downloaded updates
                    if (update.IsDownloaded)
                    {
                        pendingCount++;
                        updates.Add(new UpdateInfo
                        {
                            Title = update.Title,
                            Description = update.Description,
                            IsDownloaded = update.IsDownloaded,
                            IsMandatory = update.IsMandatory,
                            KBArticleIDs = GetKBArticles(update.KBArticleIDs),
                            MaxDownloadSize = update.MaxDownloadSize,
                            RebootRequired = update.InstallationBehavior?.RebootBehavior != WUApiLib.InstallationRebootBehavior.irbNeverReboots,
                            UpdateID = update.Identity.UpdateID
                        });
                    }
                }

                Console.WriteLine($"Found {pendingCount} pending update(s)");
            }
            catch (COMException ex)
            {
                HandleSearchError(ex, "pending updates");
            }

            return updates;
        }

        /// <summary>
        /// Gets installed updates history
        /// </summary>
        public List<UpdateHistoryInfo> GetUpdateHistory(int count = 20)
        {
            var history = new List<UpdateHistoryInfo>();
            try
            {
                Console.WriteLine($"Retrieving update history (last {count} entries)...");
                
                IUpdateSearcher searcher = _updateSession.CreateUpdateSearcher();
                int totalHistory = searcher.GetTotalHistoryCount();
                int actualCount = Math.Min(count, totalHistory);

                IUpdateHistoryEntryCollection historyCollection = searcher.QueryHistory(0, actualCount);

                foreach (IUpdateHistoryEntry entry in historyCollection)
                {
                    history.Add(new UpdateHistoryInfo
                    {
                        Title = entry.Title,
                        Date = entry.Date,
                        Operation = GetOperationText(entry.Operation),
                        ResultCode = GetResultText(entry.ResultCode),
                        Description = entry.Description,
                        UpdateID = entry.UpdateIdentity.UpdateID
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving update history: {ex.Message}");
            }

            return history;
        }

        private string GetOperationText(WUApiLib.tagUpdateOperation operation)
        {
            switch (operation)
            {
                case WUApiLib.tagUpdateOperation.uoInstallation: return "Installation";
                case WUApiLib.tagUpdateOperation.uoUninstallation: return "Uninstallation";
                default: return operation.ToString();
            }
        }

        private string GetResultText(WUApiLib.OperationResultCode resultCode)
        {
            switch (resultCode)
            {
                case WUApiLib.OperationResultCode.orcNotStarted: return "Not Started";
                case WUApiLib.OperationResultCode.orcInProgress: return "In Progress";
                case WUApiLib.OperationResultCode.orcSucceeded: return "Succeeded";
                case WUApiLib.OperationResultCode.orcSucceededWithErrors: return "Succeeded with Errors";
                case WUApiLib.OperationResultCode.orcFailed: return "Failed";
                case WUApiLib.OperationResultCode.orcAborted: return "Aborted";
                default: return resultCode.ToString();
            }
        }

        private List<string> GetKBArticles(IStringCollection kbCollection)
        {
            var articles = new List<string>();
            if (kbCollection != null)
            {
                foreach (string kb in kbCollection)
                {
                    articles.Add(kb);
                }
            }
            return articles;
        }

        private List<string> GetCategories(ICategoryCollection categories)
        {
            var categoryList = new List<string>();
            if (categories != null)
            {
                foreach (ICategory category in categories)
                {
                    categoryList.Add(category.Name);
                }
            }
            return categoryList;
        }

        private void HandleSearchError(COMException ex, string operationType)
        {
            const int WU_E_INVALID_CRITERIA = unchecked((int)0x80240032);
            const int WU_E_PT_INVALID_URL = unchecked((int)0x80240002);
            const int WU_E_NO_SERVICE = unchecked((int)0x80240437);

            Console.ForegroundColor = ConsoleColor.Red;

            if (ex.ErrorCode == WU_E_INVALID_CRITERIA)
            {
                Console.WriteLine($"Error searching for {operationType}: Invalid search criteria (0x80240032)");
                Console.WriteLine("This can occur when:");
                Console.WriteLine("  - Windows Update service is not properly initialized");
                Console.WriteLine("  - Windows Update database is corrupted");
                Console.WriteLine("  - System requires a restart");
                Console.WriteLine("\nTry running: WinUdateDiag --diagnose");
            }
            else if (ex.ErrorCode == WU_E_PT_INVALID_URL)
            {
                Console.WriteLine($"Error searching for {operationType}: Invalid update server URL (0x80240002)");
                Console.WriteLine("Check your WSUS/Windows Update configuration.");
            }
            else if (ex.ErrorCode == WU_E_NO_SERVICE)
            {
                Console.WriteLine($"Error searching for {operationType}: Windows Update service is not available (0x80240437)");
                Console.WriteLine("Ensure the Windows Update service is running.");
            }
            else
            {
                Console.WriteLine($"Error searching for {operationType}: {ex.Message} (0x{ex.ErrorCode:X})");
            }

            Console.ResetColor();
        }
    }

    public class UpdateInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDownloaded { get; set; }
        public bool IsMandatory { get; set; }
        public List<string> KBArticleIDs { get; set; }
        public decimal MaxDownloadSize { get; set; }
        public decimal MinDownloadSize { get; set; }
        public bool RebootRequired { get; set; }
        public string SeverityLevel { get; set; }
        public string UpdateID { get; set; }
        public string SupportUrl { get; set; }
        public List<string> Categories { get; set; }

        public override string ToString()
        {
            string kb = KBArticleIDs != null && KBArticleIDs.Count > 0 
                ? $"KB{string.Join(", KB", KBArticleIDs)}" 
                : "N/A";
            
            double sizeMB = (double)MaxDownloadSize / (1024 * 1024);
            string size = sizeMB > 0 ? $"{sizeMB:F2} MB" : "N/A";
            
            return $"  Title: {Title}\n" +
                   $"  KB: {kb}\n" +
                   $"  Downloaded: {IsDownloaded}\n" +
                   $"  Mandatory: {IsMandatory}\n" +
                   $"  Size: {size}\n" +
                   $"  Reboot Required: {RebootRequired}\n" +
                   $"  Severity: {SeverityLevel ?? "N/A"}\n" +
                   $"  Categories: {(Categories != null && Categories.Count > 0 ? string.Join(", ", Categories) : "N/A")}";
        }
    }

    public class UpdateHistoryInfo
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Operation { get; set; }
        public string ResultCode { get; set; }
        public string Description { get; set; }
        public string UpdateID { get; set; }

        public override string ToString()
        {
            return $"  [{Date:yyyy-MM-dd HH:mm:ss}] {Operation} - {ResultCode}\n" +
                   $"  {Title}";
        }
    }
}
