using EasySave.Core.Models;

namespace EasySave.Core.Commands
{
    /// <summary>
    /// Command for updating an existing backup job.
    /// Non-null parameters that match a valid type will be updated, while others remain unchanged.
    /// </summary>
    public class UpdateJobCommand : BackupCommand
    {
        /// <summary>
        /// Index of the job to be updated in the backup manager's job list.
        /// </summary>
        private int _indexToUpdate;

        /// <summary>
        /// New name for the backup job (optional).
        /// </summary>
        private string? _newName;

        /// <summary>
        /// New source directory path for the backup job (optional).
        /// </summary>
        private string? _newSource;

        /// <summary>
        /// New target directory path for the backup job (optional).
        /// </summary>
        private string? _newTarget;

        /// <summary>
        /// New backup type for the job (Complete or Differential, optional).
        /// </summary>
        private BackupType? _newType;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateJobCommand"/> class.
        /// </summary>
        /// <param name="backupManager">Reference to the backup manager handling jobs.</param>
        /// <param name="indexToUpdate">Index of the job to be updated.</param>
        /// <param name="newName">New name for the backup job (nullable).</param>
        /// <param name="newSource">New source directory (nullable).</param>
        /// <param name="newTarget">New target directory (nullable).</param>
        /// <param name="newType">New backup type (nullable).</param>
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

        /// <summary>
        /// Executes the update command, modifying the backup job with the provided parameters.
        /// </summary>
        public override void Execute()
        {
            _backupManager.UpdateBackupJob(_indexToUpdate, _newName, _newSource, _newTarget, _newType);
        }
    }
}
