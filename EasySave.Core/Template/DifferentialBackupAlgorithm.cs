using System.Diagnostics;
using EasySave.Core.Models;
using EasySave.Core.Models.BackupStrategies;
using EasySaveLogs;
using CryptoSoftLib;

namespace EasySave.Core.Template
{
    public class DifferentialBackupAlgorithm : AbstractBackupAlgorithm
    {
        public DifferentialBackupAlgorithm(Logger logger, Action<BackupState>? notifyObserver, Action? saveChanges, string businessSoftwareName)
            : base(logger, notifyObserver, saveChanges, businessSoftwareName)
        {
        }

        protected override bool ShouldCopyFile(string filePath, BackupJob job)
        {
            if (job._backupStrategy is DifferentialBackupStrategy diffStrategy)
            {
                var relativePath = Path.GetRelativePath(job.SourceDirectory, filePath);
                var targetFilePath = Path.Combine(job.TargetDirectory, relativePath);

                var targetDirectory = Path.GetDirectoryName(targetFilePath);
                if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                return diffStrategy.ShouldCopyFile(filePath, targetFilePath);
            }
            return false;
        }

        protected override void CopyFile(
            BackupJob job,
            string filePath,
            ref int filesProcessed,
            ref long bytesProcessed,
            int totalFiles,
            long totalSize)
        {
            // Déterminer le chemin relatif et la destination du fichier
            var relativePath = Path.GetRelativePath(job.SourceDirectory, filePath);
            var targetFilePath = Path.Combine(job.TargetDirectory, relativePath);

            // Créer le répertoire cible s'il n'existe pas
            var targetDirectory = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Copier le fichier
                File.Copy(filePath, targetFilePath, true);
                stopwatch.Stop();

                var fileSize = new FileInfo(filePath).Length;
                filesProcessed++;
                bytesProcessed += fileSize;

                // Mise à jour de l'état : on met à jour TotalFiles et RemainingFiles
                BackupState updatedState = new BackupState
                {
                    JobId = job.Id,
                    BackupName = job.Name,
                    Status = "En cours", // Obligatoire
                    LastActionTime = DateTime.Now,
                    CurrentSourceFile = filePath,
                    CurrentTargetFile = targetFilePath,
                    TotalFiles = totalFiles,
                    RemainingFiles = totalFiles - filesProcessed
                };
                // Notifier les observateurs via la méthode protégée
                Notify(updatedState);

                // Vérifier si l'extension du fichier doit être cryptée
                int encryptionTime = 0;
                var fileExtension = Path.GetExtension(filePath);
                var encryptionExtensions = ConfigurationProvider.EncryptionExtensions;

                if (encryptionExtensions.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        string encryptionKey = ConfigurationProvider.EncryptionKey;
                        encryptionTime = CryptoSoft.EncryptFile(targetFilePath, encryptionKey);
                        Console.WriteLine($"🔐 Fichier crypté : {targetFilePath} en {encryptionTime}ms");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erreur de cryptage sur {targetFilePath} : {ex.Message}");
                        encryptionTime = -1;
                    }
                }
                else
                {
                    Console.WriteLine($"⏩ Fichier ignoré pour cryptage : {targetFilePath}");
                }

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
