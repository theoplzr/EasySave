using EasySaveApp.Models;

namespace EasySaveApp.Commands
{
    /// <summary>
    /// Commande pour supprimer un job de sauvegarde à un index donné.
    /// </summary>
    public class RemoveJobCommand : BackupCommand
    {
        private int _indexToRemove;

        public RemoveJobCommand(BackupManager backupManager, int indexToRemove)
            : base(backupManager)
        {
            _indexToRemove = indexToRemove;
        }

        public override void Execute()
        {
            _backupManager.RemoveBackupJob(_indexToRemove);
        }
    }
}
