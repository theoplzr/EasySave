namespace EasySaveApp.Commands
{
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
