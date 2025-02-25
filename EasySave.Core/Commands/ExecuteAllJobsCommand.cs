namespace EasySave.Core.Commands
{
    /// <summary>
    /// Command to execute all configured backup jobs.
    /// Implements <see cref="BackupCommand"/>.
    /// </summary>
    public class ExecuteAllJobsCommand : BackupCommand
    {
        public ExecuteAllJobsCommand(BackupManager backupManager)
            : base(backupManager)
        {
        }

        public override void Execute()
        {
            Task.Run(() => _backupManager.ExecuteAllJobsAsync());
        }
    }
}
