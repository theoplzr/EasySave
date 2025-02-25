namespace EasySave.Core.Commands
{
    /// <summary>
    /// Command to execute a specific backup job by its index.
    /// Implements <see cref="BackupCommand"/>.
    /// </summary>
    public class ExecuteJobCommand : BackupCommand
    {
        private int _jobIndex;

        public ExecuteJobCommand(BackupManager backupManager, int jobIndex)
            : base(backupManager)
        {
            _jobIndex = jobIndex;
        }

        public override void Execute()
        {
            _backupManager.ExecuteBackupByIndex(_jobIndex);
        }
    }
}
