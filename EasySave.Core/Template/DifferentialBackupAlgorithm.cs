using System.Diagnostics;
using EasySave.Core.Models;
using EasySave.Core.Models.BackupStrategies;
using EasySaveLogs;
using EasySave.Core.Services; 

namespace EasySave.Core.Template
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

                var targetDirectory = Path.GetDirectoryName(targetFilePath);
                if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

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
            // Calcul du chemin relatif et détermination du chemin de destination
            var relativePath = Path.GetRelativePath(job.SourceDirectory, filePath);
            var targetFilePath = Path.Combine(job.TargetDirectory, relativePath);

            // Création du répertoire cible s'il n'existe pas
            var targetDirectory = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Effectue la copie du fichier
                File.Copy(filePath, targetFilePath, true);
                stopwatch.Stop();

                var fileSize = new FileInfo(filePath).Length;
                filesProcessed++;
                bytesProcessed += fileSize;

                // Vérifier si le fichier doit être crypté en fonction de son extension
                int encryptionTime = 0;
                var fileExtension = Path.GetExtension(filePath);
                var encryptionExtensions = ConfigurationProvider.EncryptionExtensions; // ex: [".txt", ".docx", ...]
                if (encryptionExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                {
                    // Appel au service de cryptage et récupération du temps de cryptage (en ms)
                    encryptionTime = Services.EncryptionService.EncryptFile(filePath);
                }

                // Enregistrement des informations de transfert dans le log incluant le temps de cryptage
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

                Console.WriteLine($"[Diff] Copied {filePath} -> {targetFilePath}");

                // Notifie les observateurs de l'état actuel de la sauvegarde
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

                // ---- Vérification du logiciel métier après la copie ----
                if (BusinessSoftwareChecker.IsBusinessSoftwareRunning(ConfigurationProvider.BusinessSoftware))
                {
                    LogAction(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = job.Name,
                        SourceFilePath = filePath,
                        TargetFilePath = "N/A",
                        FileSize = 0,
                        TransferTimeMs = 0,
                        EncryptionTimeMs = 0,
                        Status = $"Stopped mid-backup: Business software '{ConfigurationProvider.BusinessSoftware}' detected",
                        Level = Logger.LogLevel.Warning
                    });

                    throw new OperationCanceledException($"Backup stopped: {ConfigurationProvider.BusinessSoftware} detected (Diff).");
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // En cas d'erreur, on loggue avec des valeurs négatives pour les temps
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

                Console.WriteLine($"[Diff] Error copying {filePath}: {ex.Message}");
            }
        }
    }
}
