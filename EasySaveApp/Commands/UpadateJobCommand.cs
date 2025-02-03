using EasySaveApp.Models;

namespace EasySaveApp.Commands
{
    /// <summary>
    /// Commande pour mettre à jour un job de sauvegarde.
    /// Les paramètres non null/ne correspondant pas à un type correct ne seront pas mis à jour.
    /// </summary>
    public class UpdateJobCommand : BackupCommand
    {
        private int _indexToUpdate; // Index of the job to be updated
        private string? _newName; // New name for the job
        private string? _newSource; // New source path for the job
        private string? _newTarget; // New target path for the job
        private BackupType? _newType; // New type for the job

        // Constructor to initialize the UpdateJobCommand with the backup manager and new job details
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
