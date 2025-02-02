namespace EasySaveApp.Commands
{
    public class ExecuteAllJobsCommand : BackupCommand
    {
        public ExecuteAllJobsCommand(BackupManager backupManager)
            : base(backupManager)
        {
        }

        public override void Execute()
        {
            _backupManager.ExecuteAllJobs();
        }
    }
}
