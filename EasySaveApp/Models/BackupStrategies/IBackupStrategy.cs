// interface pour les stratégies de sauvegarde

namespace EasySaveApp.Models.BackupStrategies
{
    public interface IBackupStrategy
    {
        bool ShouldCopyFile(string sourceFilePath, string targetFilePath);
    }
}
