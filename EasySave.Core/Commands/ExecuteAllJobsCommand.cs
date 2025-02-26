using System.Threading.Tasks;

namespace EasySave.Core.Commands
{
    /// <summary>
    /// Command to execute all configured backup jobs.
    /// Implements <see cref="BackupCommand"/>.
    /// </summary>
    public class ExecuteAllJobsCommand : BackupCommand
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ExecuteAllJobsCommand"/>.
        /// </summary>
        /// <param name="backupManager">The manager responsible for executing backup operations.</param>
        public ExecuteAllJobsCommand(BackupManager backupManager)
            : base(backupManager)
        {
        }

        /// <summary>
        /// Initiates the execution of all backup jobs asynchronously.
        /// </summary>
        public override void Execute()
        {
            // Run the backup process on a separate thread to avoid blocking the caller.
            Task.Run(() => _backupManager.ExecuteAllJobsAsync());
        }
    }
}
