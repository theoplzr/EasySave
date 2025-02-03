using EasySaveApp.Models;

namespace EasySaveApp.Commands
{
    /// <summary>
    /// Command to remove a backup job at a specified index.
    /// Implements <see cref="BackupCommand"/>.
    /// </summary>
    public class RemoveJobCommand : BackupCommand
    {
        /// <summary>
        /// Index of the backup job to be removed.
        /// </summary>
        private int _indexToRemove;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveJobCommand"/> class.
        /// </summary>
        /// <param name="backupManager">Reference to the backup manager handling jobs.</param>
        /// <param name="indexToRemove">Index of the backup job to be removed.</param>
        public RemoveJobCommand(BackupManager backupManager, int indexToRemove)
            : base(backupManager)
        {
            _indexToRemove = indexToRemove;
        }

        /// <summary>
        /// Executes the command to remove the specified backup job.
        /// </summary>
        public override void Execute()
        {
            _backupManager.RemoveBackupJob(_indexToRemove);
        }
    }
}
