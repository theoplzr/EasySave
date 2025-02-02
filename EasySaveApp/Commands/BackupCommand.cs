namespace EasySaveApp.Commands
{
    public abstract class BackupCommand : ICommand
    {
        protected BackupManager _backupManager;

        protected BackupCommand(BackupManager backupManager)
        {
            _backupManager = backupManager;
        }

        // Méthode que les commandes concrètes devront implémenter
        public abstract void Execute();
    }
}
