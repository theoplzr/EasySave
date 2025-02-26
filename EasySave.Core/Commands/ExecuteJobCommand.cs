namespace EasySave.Core.Commands
{
    /// <summary>
    /// Command to execute a specific backup job by its index.
    /// Implements <see cref="BackupCommand"/>.
    /// </summary>
    public class ExecuteJobCommand : BackupCommand
    {
        // Index of the backup job to be executed.
        private readonly int _jobIndex;

        /// <summary>
        /// Initializes a new instance of <see cref="ExecuteJobCommand"/>.
        /// </summary>
        /// <param name="backupManager">The manager responsible for executing backup operations.</param>
        /// <param name="jobIndex">The index of the job to execute.</param>
        public ExecuteJobCommand(BackupManager backupManager, int jobIndex)
            : base(backupManager)
        {
            _jobIndex = jobIndex;
        }

        /// <summary>
        /// Executes the backup job that corresponds to the specified index.
        /// </summary>
        public override void Execute()
        {
            _backupManager.ExecuteBackupByIndex(_jobIndex);
        }
    }
}
