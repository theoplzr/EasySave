using EasySaveApp.Models;

namespace EasySaveApp.Commands
{
    /// <summary>
    /// Command to add a new backup job.
    /// Implements <see cref="BackupCommand"/>.
    /// </summary>
    public class AddJobCommand : BackupCommand
    {
        /// <summary>
        /// The backup job to be added.
        /// </summary>
        private BackupJob _jobToAdd;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddJobCommand"/> class.
        /// </summary>
        /// <param name="backupManager">Reference to the backup manager handling jobs.</param>
        /// <param name="jobToAdd">The backup job to be added.</param>
        public AddJobCommand(BackupManager backupManager, BackupJob jobToAdd)
            : base(backupManager)
        {
            _jobToAdd = jobToAdd;
        }

        /// <summary>
        /// Executes the command to add the backup job.
        /// </summary>
        public override void Execute()
        {
            _backupManager.AddBackupJob(_jobToAdd);
        }
    }
}
