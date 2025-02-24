using System.Diagnostics;
using EasySaveLogs;
using EasySave.Core.Utils;

namespace EasySave.Core.Services
{
    public class BusinessSoftwareChecker
    {
        public static bool IsBusinessSoftwareRunning(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                bool isRunning = processes.Any();

                if (isRunning)
                {
                    string logDirectory = Configuration.GetLogDirectory();
                    string logFormat = Configuration.GetLogFormat();
                    Logger.GetInstance(logDirectory, logFormat).LogAction(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = "Job Interrompu",
                        SourceFilePath = "N/A",
                        TargetFilePath = "N/A",
                        FileSize = 0,
                        TransferTimeMs = 0,
                        EncryptionTimeMs = 0,
                        Status = $"Sauvegarde interrompue - {processName} détecté",
                        Level = Logger.LogLevel.Warning
                    });
                }

                return isRunning;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
