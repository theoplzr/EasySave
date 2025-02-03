namespace EasySaveApp.Models.BackupStrategies
{
    /// <summary>
    /// Implements a differential backup strategy.
    /// In a differential backup, only files that have changed since the last full backup are copied.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Determines whether a file should be copied based on its last modified time.
        /// </summary>
        /// <param name="sourceFilePath">The path of the source file.</param>
        /// <param name="targetFilePath">The path of the target file.</param>
        /// <returns>Returns true if the file has been modified since the last backup, otherwise false.</returns>
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
