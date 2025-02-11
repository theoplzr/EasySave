using System.Diagnostics;
using EasySaveLogs;
using EasySave.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace EasySave.Core.Services
{
    public class EncryptionService
    {
        public static int EncryptFile(string filePath)
        {
            try
            {
                // Vérifier si l'extension du fichier est autorisée
                List<string> extensionsAutorisees = Configuration.GetCryptoExtensions();
                if (!extensionsAutorisees.Contains(Path.GetExtension(filePath).ToLower()))
                {
                    return 0; // Pas de cryptage nécessaire
                }

                var stopwatch = Stopwatch.StartNew();

                // --- Appel à CryptoSoft ---
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "CryptoSoft.exe",
                    Arguments = $"\"{filePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                try
                {
                    using (var process = Process.Start(psi))
                    {
                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    string logDirectory = Configuration.GetLogDirectory();
                    string logFormat = Configuration.GetLogFormat();
                    Logger.GetInstance(logDirectory, logFormat).LogAction(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = "CryptoSoft",
                        SourceFilePath = filePath,
                        TargetFilePath = "N/A",
                        FileSize = 0,
                        TransferTimeMs = 0,
                        EncryptionTimeMs = -1,
                        Status = $"Erreur : CryptoSoft n'a pas pu être exécuté ({ex.Message})",
                        Level = Logger.LogLevel.Error
                    });

                    return -1;
                }

                stopwatch.Stop();
                int encryptionTimeMs = (int)stopwatch.ElapsedMilliseconds;

                string finalLogDirectory = Configuration.GetLogDirectory();
                string finalLogFormat = Configuration.GetLogFormat();
                Logger.GetInstance(finalLogDirectory, finalLogFormat).LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = "Cryptage",
                    SourceFilePath = filePath,
                    TargetFilePath = "N/A",
                    FileSize = new FileInfo(filePath).Length,
                    TransferTimeMs = 0,
                    EncryptionTimeMs = encryptionTimeMs,
                    Status = "Fichier crypté avec succès",
                    Level = Logger.LogLevel.Info
                });

                return encryptionTimeMs;
            }
            catch (Exception ex)
            {
                string logDirectory = Configuration.GetLogDirectory();
                string logFormat = Configuration.GetLogFormat();
                Logger.GetInstance(logDirectory, logFormat).LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = "Erreur Cryptage",
                    SourceFilePath = filePath,
                    TargetFilePath = "N/A",
                    FileSize = 0,
                    TransferTimeMs = 0,
                    EncryptionTimeMs = -1,
                    Status = $"Erreur de cryptage : {ex.Message}",
                    Level = Logger.LogLevel.Error
                });

                return -1;
            }
        }
    }
}
