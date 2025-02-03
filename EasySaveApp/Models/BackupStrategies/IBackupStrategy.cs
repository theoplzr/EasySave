namespace EasySaveApp.Models.BackupStrategies
{
    /// <summary>
    /// Interface defining a backup strategy.
    /// Determines whether a file should be copied during a backup process.
    /// </summary>
    public interface IBackupStrategy
    {
        /// <summary>
        /// Determines whether a file should be copied based on the backup strategy.
        /// </summary>
        /// <param name="sourceFilePath">The path of the source file.</param>
        /// <param name="targetFilePath">The path of the target file.</param>
        /// <returns>Returns true if the file should be copied, otherwise false.</returns>
        bool ShouldCopyFile(string sourceFilePath, string targetFilePath);
    }
}
