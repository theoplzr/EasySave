//classe qui permet de créer une stratégie de sauvegarde en fonction du type de sauvegarde

namespace EasySaveApp.Models.BackupStrategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        public bool ShouldCopyFile(string sourceFilePath, string targetFilePath)
        {
            return true; // Toujours copier les fichiers en sauvegarde complète
        }
    }
}
