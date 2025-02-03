using System.Diagnostics;
using EasySaveApp.Models;
using EasySaveLogs;

namespace EasySaveApp.Template
{
    /// <summary>
    /// Implements a full backup algorithm.
    /// Copies all files from the source directory to the target directory.
    /// </summary>
    public class FullBackupAlgorithm : AbstractBackupAlgorithm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FullBackupAlgorithm"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for recording actions.</param>
        /// <param name="notifyObserver">Action to notify observers of backup state changes.</param>
        /// <param name="saveChanges">Action to persist backup state changes.</param>
        public FullBackupAlgorithm(Logger logger, Action<BackupState>? notifyObserver, Action? saveChanges)
            : base(logger, notifyObserver, saveChanges)
        {
        }

        /// <summary>
        /// Determines whether a file should be copied.
        /// Since this is a full backup, all files are always copied.
        /// </summary>
        /// <param name="filePath">The path of the file to check.</param>
        /// <param name="job">The backup job being processed.</param>
        /// <returns>Always returns true to copy all files.</returns>
        protected override bool ShouldCopyFile(string filePath, BackupJob job)
        {
            return true; // In a full backup, all files are copied.
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

                Console.WriteLine($"[Full] Copied {filePath} -> {targetFilePath}");

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

                Console.WriteLine($"[Full] Error copying {filePath}: {ex.Message}");
            }
        }
    }
}
