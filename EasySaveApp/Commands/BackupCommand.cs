namespace EasySaveApp.Commands
{
    /// <summary>
    /// Abstract base class for all backup-related commands.
    /// Implements <see cref="ICommand"/> and provides a reference to the backup manager.
    /// </summary>
    public abstract class BackupCommand : ICommand
    {
        /// <summary>
        /// Reference to the backup manager that handles backup jobs.
        /// </summary>
        protected BackupManager _backupManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupCommand"/> class.
        /// </summary>
        /// <param name="backupManager">Reference to the backup manager handling jobs.</param>
        protected BackupCommand(BackupManager backupManager)
        {
            _backupManager = backupManager;
        }

        /// <summary>
        /// Executes the command. This method must be implemented by derived classes.
        /// </summary>
        public abstract void Execute();
    }
}
