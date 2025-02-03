namespace EasySaveApp.Models.BackupStrategies
{
    /// <summary>
    /// Implements a full backup strategy.
    /// In a full backup, all files are always copied regardless of their modification date.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Determines whether a file should be copied.
        /// Since this is a full backup, all files are copied unconditionally.
        /// </summary>
        /// <param name="sourceFilePath">The path of the source file.</param>
        /// <param name="targetFilePath">The path of the target file.</param>
        /// <returns>Always returns true, meaning all files are copied.</returns>
        public bool ShouldCopyFile(string sourceFilePath, string targetFilePath)
        {
            return true; // Always copy files in a full backup
        }
    }
}
