// interface pour les stratégies de sauvegarde

namespace EasySaveApp.Models.BackupStrategies
{
    public interface IBackupStrategy
    {
        // Method to determine if a file should be copied
        bool ShouldCopyFile(string sourceFilePath, string targetFilePath);
    }
}