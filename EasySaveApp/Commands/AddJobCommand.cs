using EasySaveApp.Models;

namespace EasySaveApp.Commands
{
    public class AddJobCommand : BackupCommand
    {
        private BackupJob _jobToAdd; // The backup job to be added

        // Constructor to initialize the AddJobCommand with the backup manager and the job to add
        public AddJobCommand(BackupManager backupManager, BackupJob jobToAdd)
            : base(backupManager)
        {
            _jobToAdd = jobToAdd;
        }

        // Method to execute the command to add the backup job
        public override void Execute()
        {
            _backupManager.AddBackupJob(_jobToAdd);
        }
    }
}