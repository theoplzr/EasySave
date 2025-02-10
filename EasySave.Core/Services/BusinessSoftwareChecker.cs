using System.Diagnostics;

namespace EasySave.Core.Services
{
    public class BusinessSoftwareChecker
    {
        /// <summary>
        /// Vérifie si un logiciel métier (dont le nom est passé en paramètre) est en cours d'exécution.
        /// </summary>
        /// <param name="processName">Nom du logiciel métier à détecter</param>
        /// <returns>True si le processus est trouvé, sinon false.</returns>
        public static bool IsBusinessSoftwareRunning(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                bool isRunning = processes.Any();

                if (isRunning)
                {
                    Logger.GetInstance().LogAction(new LogEntry
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
                Logger.GetInstance().LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = "Erreur Détection Logiciel Métier",
                    SourceFilePath = "N/A",
                    TargetFilePath = "N/A",
                    FileSize = 0,
                    TransferTimeMs = 0,
                    EncryptionTimeMs = 0,
                    Status = $"Erreur lors de la détection du logiciel métier : {ex.Message}",
                    Level = Logger.LogLevel.Error
                });

                return false;
            }
        }
    }
}
