using EasySaveApp.Models;

namespace EasySaveApp.Commands
{
    /// <summary>
    /// Commande pour mettre à jour un job de sauvegarde.
    /// Les paramètres non null/ne correspondant pas à un type correct ne seront pas mis à jour.
    /// </summary>
    public class UpdateJobCommand : BackupCommand
    {
        private int _indexToUpdate;
        private string? _newName;
        private string? _newSource;
        private string? _newTarget;
        private BackupType? _newType;

        public UpdateJobCommand(
            BackupManager backupManager,
            int indexToUpdate,
            string? newName,
            string? newSource,
            string? newTarget,
            BackupType? newType)
            : base(backupManager)
        {
            _indexToUpdate = indexToUpdate;
            _newName = newName;
            _newSource = newSource;
            _newTarget = newTarget;
            _newType = newType;
        }

        public override void Execute()
        {
            _backupManager.UpdateBackupJob(_indexToUpdate, _newName, _newSource, _newTarget, _newType);
        }
    }
}
