namespace EasySaveApp.Commands
{
    /// <summary>
    /// Command to execute all configured backup jobs.
    /// Implements <see cref="BackupCommand"/>.
    /// </summary>
    public class ExecuteAllJobsCommand : BackupCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteAllJobsCommand"/> class.
        /// </summary>
        /// <param name="backupManager">Reference to the backup manager handling jobs.</param>
        public ExecuteAllJobsCommand(BackupManager backupManager)
            : base(backupManager)
        {
        }

        /// <summary>
        /// Executes the command to run all backup jobs.
        /// </summary>
        public override void Execute()
        {
            _backupManager.ExecuteAllJobs();
        }
    }
}
