using EasySaveApp.Models;

namespace EasySaveApp.Commands
{
    /// <summary>
    /// Commande pour supprimer un job de sauvegarde à un index donné.
    /// </summary>
    public class RemoveJobCommand : BackupCommand
    {
        private int _indexToRemove; // Index of the job to be removed

        // Constructor to initialize the RemoveJobCommand with the backup manager and index to remove
        public RemoveJobCommand(BackupManager backupManager, int indexToRemove)
            : base(backupManager)
        {
            _indexToRemove = indexToRemove;
        }

        // Method to execute the command to remove the specified backup job
        public override void Execute()
        {
            _backupManager.RemoveBackupJob(_indexToRemove);
        }
    }
}