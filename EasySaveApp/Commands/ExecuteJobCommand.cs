namespace EasySaveApp.Commands
{
    /// <summary>
    /// Command to execute a specific backup job by its index.
    /// Implements <see cref="BackupCommand"/>.
    /// </summary>
    public class ExecuteJobCommand : BackupCommand
    {
        /// <summary>
        /// Index of the backup job to be executed.
        /// </summary>
        private int _jobIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteJobCommand"/> class.
        /// </summary>
        /// <param name="backupManager">Reference to the backup manager handling jobs.</param>
        /// <param name="jobIndex">Index of the backup job to execute.</param>
        public ExecuteJobCommand(BackupManager backupManager, int jobIndex)
            : base(backupManager)
        {
            _jobIndex = jobIndex;
        }

        /// <summary>
        /// Executes the command to run the specified backup job.
        /// </summary>
        public override void Execute()
        {
            _backupManager.ExecuteBackupByIndex(_jobIndex);
        }
    }
}