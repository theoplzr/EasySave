//classe qui permet de créer une stratégie de sauvegarde en fonction du type de sauvegarde

namespace EasySaveApp.Models.BackupStrategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public bool ShouldCopyFile(string sourceFilePath, string targetFilePath)
        {
            if (!File.Exists(targetFilePath)) return true;

            var sourceLastModified = File.GetLastWriteTime(sourceFilePath);
            var targetLastModified = File.GetLastWriteTime(targetFilePath);

            return sourceLastModified > targetLastModified;
        }
    }
}
