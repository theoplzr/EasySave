namespace EasySaveApp.Commands
{
    public abstract class BackupCommand : ICommand
    {
        protected BackupManager _backupManager; // The backup manager to manage backup jobs

        // Constructor to initialize the BackupCommand with the backup manager
        protected BackupCommand(BackupManager backupManager)
        {
            _backupManager = backupManager;
        }

        // Method that concrete commands will need to implement
        public abstract void Execute();
    }
}