using System.Diagnostics;
using EasySaveApp.Models;
using EasySaveLogs;

namespace EasySaveApp.Template
{
    public class FullBackupAlgorithm : AbstractBackupAlgorithm
    {
        public FullBackupAlgorithm(Logger logger, Action<BackupState>? notifyObserver, Action? saveChanges)
            : base(logger, notifyObserver, saveChanges)
        {
        }

        protected override bool ShouldCopyFile(string filePath, BackupJob job)
        {
            // Pour une sauvegarde complète, on copie toujours.
            return true;
        }

        protected override void CopyFile(
            BackupJob job, 
            string filePath, 
            ref int filesProcessed, 
            ref long bytesProcessed,
            int totalFiles,
            long totalSize
        )
        {
            var relativePath = Path.GetRelativePath(job.SourceDirectory, filePath);
            var targetFilePath = Path.Combine(job.TargetDirectory, relativePath);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                File.Copy(filePath, targetFilePath, true);
                stopwatch.Stop();

                var fileSize = new FileInfo(filePath).Length;
                filesProcessed++;
                bytesProcessed += fileSize;

                // Log
                LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    SourceFilePath = filePath,
                    TargetFilePath = targetFilePath,
                    FileSize = fileSize,
                    TransferTimeMs = stopwatch.ElapsedMilliseconds,
                    Status = "Success"
                });

                Console.WriteLine($"[Full] Copied {filePath} -> {targetFilePath}");

                // Notifier l'état
                Notify(new BackupState
                {
                    JobId = job.Id,
                    BackupName = job.Name,
                    LastActionTime = DateTime.Now,
                    Status = "Actif",
                    TotalFiles = totalFiles,
                    TotalSize = totalSize,
                    RemainingFiles = totalFiles - filesProcessed,
                    RemainingSize = totalSize - bytesProcessed,
                    CurrentSourceFile = filePath,
                    CurrentTargetFile = targetFilePath
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    SourceFilePath = filePath,
                    TargetFilePath = targetFilePath,
                    FileSize = 0,
                    TransferTimeMs = -1,
                    Status = "Error: " + ex.Message
                });
                Console.WriteLine($"[Full] Error copying {filePath}: {ex.Message}");
            }
        }
    }
}
