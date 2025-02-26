using CryptoSoftLib;
using EasySave.Core.Models;
using EasySave.Core.Models.BackupStrategies;
using EasySaveLogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasySave.Core.Template
{
    /// <summary>
    /// Represents a differential backup algorithm, which copies only files that have changed.
    /// </summary>
    public class DifferentialBackupAlgorithm : AbstractBackupAlgorithm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentialBackupAlgorithm"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for recording actions.</param>
        /// <param name="notifyObserver">Callback to notify observers about backup state changes.</param>
        /// <param name="saveChanges">Callback to persist state changes.</param>
        /// <param name="businessSoftwareName">Name of the business software to detect for interruption.</param>
        public DifferentialBackupAlgorithm(
            Logger logger,
            Action<BackupState>? notifyObserver,
            Action? saveChanges,
            string businessSoftwareName
        )
            : base(logger, notifyObserver, saveChanges, businessSoftwareName)
        {
        }

        /// <summary>
        /// Determines if a file should be copied in a differential backup scenario.
        /// </summary>
        /// <param name="filePath">The path to the source file.</param>
        /// <param name="job">The current backup job.</param>
        /// <returns><c>true</c> if the file meets criteria to be copied; otherwise <c>false</c>.</returns>
        protected override bool ShouldCopyFile(string filePath, BackupJob job)
        {
            // The job strategy should be a DifferentialBackupStrategy; verify and use it.
            if (job._backupStrategy is DifferentialBackupStrategy diffStrategy)
            {
                var relativePath = Path.GetRelativePath(job.SourceDirectory, filePath);
                var targetFilePath = Path.Combine(job.TargetDirectory, relativePath);

                // Ensure the target directory exists
                var targetDirectory = Path.GetDirectoryName(targetFilePath);
                if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                // Let the strategy decide if the file should be copied
                return diffStrategy.ShouldCopyFile(filePath, targetFilePath);
            }
            return false;
        }

        /// <summary>
        /// Performs the actual file copying for a differential backup, and applies optional encryption.
        /// </summary>
        protected override void CopyFile(
            BackupJob job,
            string filePath,
            ref int filesProcessed,
            ref long bytesProcessed,
            int totalFiles,
            long totalSize
        )
        {
            // Determine target path
            var relativePath = Path.GetRelativePath(job.SourceDirectory, filePath);
            var targetFilePath = Path.Combine(job.TargetDirectory, relativePath);

            // Ensure the target directory exists
            var targetDirectory = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Copy file
                File.Copy(filePath, targetFilePath, true);
                stopwatch.Stop();

                var fileSize = new FileInfo(filePath).Length;
                filesProcessed++;
                bytesProcessed += fileSize;

                // Update and notify state
                var updatedState = new BackupState
                {
                    JobId = job.Id,
                    BackupName = job.Name,
                    Status = "In progress",
                    LastActionTime = DateTime.Now,
                    CurrentSourceFile = filePath,
                    CurrentTargetFile = targetFilePath,
                    TotalFiles = totalFiles,
                    RemainingFiles = totalFiles - filesProcessed
                };
                Notify(updatedState);

                // Check if the file should be encrypted
                int encryptionTime = 0;
                var fileExtension = Path.GetExtension(filePath);
                var encryptionExtensions = ConfigurationProvider.EncryptionExtensions;

                if (encryptionExtensions.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        string encryptionKey = ConfigurationProvider.EncryptionKey;
                        encryptionTime = CryptoSoft.EncryptFile(targetFilePath, encryptionKey);
                        Console.WriteLine($"🔐 Encrypted: {targetFilePath} in {encryptionTime}ms");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Encryption error on {targetFilePath}: {ex.Message}");
                        encryptionTime = -1;
                    }
                }
                else
                {
                    Console.WriteLine($"⏩ Skipped encryption for: {targetFilePath}");
                }

                // Log action
                LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    SourceFilePath = filePath,
                    TargetFilePath = targetFilePath,
                    FileSize = fileSize,
                    TransferTimeMs = stopwatch.ElapsedMilliseconds,
                    EncryptionTimeMs = encryptionTime,
                    Status = "Success"
                });

                Console.WriteLine($"✅ [Diff] Copied {filePath} -> {targetFilePath}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log the error
                LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    SourceFilePath = filePath,
                    TargetFilePath = targetFilePath,
                    FileSize = 0,
                    TransferTimeMs = -1,
                    EncryptionTimeMs = -1,
                    Status = "Error: " + ex.Message
                });

                Console.WriteLine($"❌ [Diff] Error copying {filePath}: {ex.Message}");
            }
        }
    }
}
