// classe qui permet de créer une stratégie de sauvegarde en fonction du type de sauvegarde

namespace EasySaveApp.Models.BackupStrategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        // Method to determine if a file should be copied
        public bool ShouldCopyFile(string sourceFilePath, string targetFilePath)
        {
            return true; // Always copy files in a full backup
        }
    }
}