namespace EasySaveApp.Commands
{
    public class ExecuteJobCommand : BackupCommand
    {
        private int _jobIndex; // Index of the job to be executed

        // Constructor to initialize the ExecuteJobCommand with the backup manager and job index
        public ExecuteJobCommand(BackupManager backupManager, int jobIndex)
            : base(backupManager)
        {
            _jobIndex = jobIndex;
        }

        // Method to execute the command to run the specified backup job
        public override void Execute()
        {
            _backupManager.ExecuteBackupByIndex(_jobIndex);
        }
    }
}