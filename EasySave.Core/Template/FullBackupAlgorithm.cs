using System.Diagnostics;
using EasySave.Core.Models;
using EasySaveLogs;
using CryptoSoftLib;

namespace EasySave.Core.Template
{
    public class FullBackupAlgorithm : AbstractBackupAlgorithm
    {
        public FullBackupAlgorithm(Logger logger, Action<BackupState>? notifyObserver, Action? saveChanges)
            : base(logger, notifyObserver, saveChanges)
        {
            
        }

        protected override bool ShouldCopyFile(string filePath, BackupJob job)
        {
            return true; 
        }

        protected override void CopyFile(
            BackupJob job,
            string filePath,
            ref int filesProcessed,
            ref long bytesProcessed,
            int totalFiles,
            long totalSize)
        {
            var relativePath = Path.GetRelativePath(job.SourceDirectory, filePath);
            var targetFilePath = Path.Combine(job.TargetDirectory, relativePath);

            var targetDirectory = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                File.Copy(filePath, targetFilePath, true);
                stopwatch.Stop();

                var fileSize = new FileInfo(filePath).Length;
                filesProcessed++;
                bytesProcessed += fileSize;

                // Vérifier si l'extension du fichier est dans la liste des fichiers à crypter
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
                        encryptionTime = -1; // Code d'erreur pour le log
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

                Console.WriteLine($"✅ [Full] Copied {filePath} -> {targetFilePath}");
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

                Console.WriteLine($"❌ [Full] Error copying {filePath}: {ex.Message}");
            }
        }
    }
}
