using EasySaveApp.Models;

namespace EasySaveApp.Commands
{
    public class AddJobCommand : BackupCommand
    {
        private BackupJob _jobToAdd;

        public AddJobCommand(BackupManager backupManager, BackupJob jobToAdd)
            : base(backupManager)
        {
            _jobToAdd = jobToAdd;
        }

        public override void Execute()
        {
            _backupManager.AddBackupJob(_jobToAdd);
        }
    }
}
