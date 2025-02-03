// classe qui permet de créer une stratégie de sauvegarde en fonction du type de sauvegarde

namespace EasySaveApp.Models.BackupStrategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        // Method to determine if a file should be copied based on its last modified time
        public bool ShouldCopyFile(string sourceFilePath, string targetFilePath)
        {
            // If the target file does not exist, the file should be copied
            if (!File.Exists(targetFilePath)) return true;

            // Get the last modified time of the source and target files
            var sourceLastModified = File.GetLastWriteTime(sourceFilePath);
            var targetLastModified = File.GetLastWriteTime(targetFilePath);

            // Copy the file if the source file is newer than the target file
            return sourceLastModified > targetLastModified;
        }
    }
}