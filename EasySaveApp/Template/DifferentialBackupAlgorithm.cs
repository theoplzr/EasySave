using System.Diagnostics;
using EasySaveApp.Models;
using EasySaveApp.Models.BackupStrategies;
using EasySaveLogs;

namespace EasySaveApp.Template
{
    /// <summary>
    /// Implements a differential backup algorithm.
    /// Copies only files that have changed since the last full backup.
    /// </summary>
    public class DifferentialBackupAlgorithm : AbstractBackupAlgorithm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentialBackupAlgorithm"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for recording actions.</param>
        /// <param name="notifyObserver">Action to notify observers of backup state changes.</param>
        /// <param name="saveChanges">Action to persist backup state changes.</param>
        public DifferentialBackupAlgorithm(Logger logger, Action<BackupState>? notifyObserver, Action? saveChanges)
            : base(logger, notifyObserver, saveChanges)
        {
        }

        /// <summary>
        /// Determines whether a file should be copied based on the differential backup strategy.
        /// </summary>
        /// <param name="filePath">The path of the file to check.</param>
        /// <param name="job">The backup job being processed.</param>
        /// <returns>Returns true if the file should be copied, otherwise false.</returns>
        protected override bool ShouldCopyFile(string filePath, BackupJob job)
        {
            // Check if the backup strategy is DifferentialBackupStrategy
            if (job._backupStrategy is DifferentialBackupStrategy diffStrategy)
            {
                var relativePath = Path.GetRelativePath(job.SourceDirectory, filePath);
                var targetFilePath = Path.Combine(job.TargetDirectory, relativePath);
                
                // Delegate the decision to the strategy
                return diffStrategy.ShouldCopyFile(filePath, targetFilePath);
            }

            // If no strategy is defined, do not copy anything
            return false;
        }

        /// <summary>
        /// Copies a file from the source directory to the target directory while logging its progress.
        /// </summary>
        /// <param name="job">The backup job being executed.</param>
        /// <param name="filePath">The path of the file to copy.</param>
        /// <param name="filesProcessed">Reference to the count of processed files.</param>
        /// <param name="bytesProcessed">Reference to the total size of processed files.</param>
        /// <param name="totalFiles">Total number of files to be processed.</param>
        /// <param name="totalSize">Total size of files to be backed up.</param>
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
                // Perform the file copy operation
                File.Copy(filePath, targetFilePath, true);
                stopwatch.Stop();

                var fileSize = new FileInfo(filePath).Length;
                filesProcessed++;
                bytesProcessed += fileSize;

                // Log the file transfer details
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

                Console.WriteLine($"[Diff] Copied {filePath} -> {targetFilePath}");

                // Notify observers about the backup state update
                Notify(new BackupState
                {
                    JobId = job.Id,
                    BackupName = job.Name,
                    LastActionTime = DateTime.Now,
                    Status = "Active",
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

                // Log the error if file copying fails
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

                Console.WriteLine($"[Diff] Error copying {filePath}: {ex.Message}");
            }
        }
    }
}
