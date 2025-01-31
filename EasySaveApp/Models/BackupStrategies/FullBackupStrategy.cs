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
