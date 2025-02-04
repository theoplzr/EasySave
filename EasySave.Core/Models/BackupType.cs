namespace EasySave.Core.Models
{
    /// <summary>
    /// Enumeration of backup types.
    /// </summary>
    public enum BackupType
    {
        /// <summary>
        /// Full backup: Copies all files regardless of previous backups.
        /// </summary>
        Complete,

        /// <summary>
        /// Differential backup: Copies only files that have changed since the last full backup.
        /// </summary>
        Differential
    }
}
