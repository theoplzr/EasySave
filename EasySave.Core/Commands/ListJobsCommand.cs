namespace EasySave.Core.Commands
{
    /// <summary>
    /// Command to list all configured backup jobs.
    /// Implements <see cref="BackupCommand"/>.
    /// </summary>
    public class ListJobsCommand : BackupCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListJobsCommand"/> class.
        /// </summary>
        /// <param name="backupManager">Reference to the backup manager handling jobs.</param>
        public ListJobsCommand(BackupManager backupManager)
            : base(backupManager)
        {
        }

        /// <summary>
        /// Executes the command to list all backup jobs.
        /// </summary>
        public override void Execute()
        {
            _backupManager.ListBackupJobs();
        }
    }
}
