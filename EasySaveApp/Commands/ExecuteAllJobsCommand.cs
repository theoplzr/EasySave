namespace EasySaveApp.Commands
{
    public class ExecuteAllJobsCommand : BackupCommand
    {
        // Constructor to initialize the ExecuteAllJobsCommand with the backup manager
        public ExecuteAllJobsCommand(BackupManager backupManager)
            : base(backupManager)
        {
        }

        // Method to execute the command to run all backup jobs
        public override void Execute()
        {
            _backupManager.ExecuteAllJobs();
        }
    }
}