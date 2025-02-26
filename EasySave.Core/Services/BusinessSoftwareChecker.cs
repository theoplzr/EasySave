using System;
using System.Diagnostics;
using System.Linq;
using EasySave.Core.Utils;
using EasySaveLogs;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Provides functionality to detect if a specified business software is currently running.
    /// </summary>
    public class BusinessSoftwareChecker
    {
        /// <summary>
        /// Checks if a given process name is currently active.
        /// If it is running, logs a warning indicating that the backup has been interrupted.
        /// </summary>
        /// <param name="processName">The name of the process to detect.</param>
        /// <returns>True if the process is running, otherwise false.</returns>
        public static bool IsBusinessSoftwareRunning(string processName)
        {
            try
            {
                // Attempt to retrieve processes matching the specified name
                var processes = Process.GetProcessesByName(processName);
                bool isRunning = processes.Any();

                // If the process is detected, log a warning
                if (isRunning)
                {
                    string logDirectory = Configuration.GetLogDirectory();
                    string logFormat = Configuration.GetLogFormat();
                    
                    Logger.GetInstance(logDirectory, logFormat).LogAction(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = "Job Interrupted",
                        SourceFilePath = "N/A",
                        TargetFilePath = "N/A",
                        FileSize = 0,
                        TransferTimeMs = 0,
                        EncryptionTimeMs = 0,
                        Status = $"Backup interrupted - {processName} detected",
                        Level = Logger.LogLevel.Warning
                    });
                }

                return isRunning;
            }
            catch (Exception)
            {
                // Return false if an exception occurs so as not to halt the backup process entirely.
                return false;
            }
        }
    }
}
