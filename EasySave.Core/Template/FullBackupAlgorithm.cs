using CryptoSoftLib;
using EasySave.Core.Models;
using EasySaveLogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasySave.Core.Template
{
    /// <summary>
    /// Represents a full backup algorithm, which copies all files regardless of modification.
    /// </summary>
    public class FullBackupAlgorithm : AbstractBackupAlgorithm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FullBackupAlgorithm"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for recording actions.</param>
        /// <param name="notifyObserver">Callback to notify observers about backup state changes.</param>
        /// <param name="saveChanges">Callback to persist state changes.</param>
        /// <param name="businessSoftwareName">Name of the business software to detect for interruption.</param>
        public FullBackupAlgorithm(
            Logger logger,
            Action<BackupState>? notifyObserver,
            Action? saveChanges,
            string businessSoftwareName
        )
            : base(logger, notifyObserver, saveChanges, businessSoftwareName)
        {
        }

        /// <summary>
        /// In a full backup, every file should be copied, so always returns <c>true</c>.
        /// </summary>
        /// <param name="filePath">The path to the source file.</param>
        /// <param name="job">The current backup job.</param>
        /// <returns>Always <c>true</c>.</returns>
        protected override bool ShouldCopyFile(string filePath, BackupJob job)
        {
            return true;
        }

        /// <summary>
        /// Performs the actual file copying for a full backup, applying optional encryption as needed.
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
                        Console.WriteLine($"Encrypted file: {targetFilePath} in {encryptionTime}ms");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Encryption error on {targetFilePath}: {ex.Message}");
                        encryptionTime = -1;
                    }
                }
                else
                {
                    Console.WriteLine($"Skipped encryption for file: {targetFilePath}");
                }

                // Log the copy action
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

                Console.WriteLine($"[Full] Copied {filePath} -> {targetFilePath}");
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

                Console.WriteLine($"[Full] Error copying {filePath}: {ex.Message}");
            }
        }
    }
}
