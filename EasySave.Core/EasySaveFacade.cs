using EasySave.Core.Commands;
using EasySave.Core.Models;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;

namespace EasySave.Core.Facade
{
    /// <summary>
    /// Provides a simplified interface (facade) for performing the main operations of EasySave
    /// without exposing internal implementation details.
    /// Implements the **Facade Design Pattern** to streamline interactions with the backup system.
    /// </summary>
    public class EasySaveFacade
    {
        /// <summary>
        /// Instance of the <see cref="BackupManager"/> that handles backup jobs.
        /// </summary>
        private readonly BackupManager _backupManager;

        /// <summary>
        /// Initializes the EasySave facade with the required dependencies:
        /// - A repository for backup job persistence.
        /// - A log directory for logging backup activities.
        /// - An optional observer for tracking backup state.
        /// </summary>
        /// <param name="jobRepository">The repository for managing backup job persistence.</param>
        /// <param name="logDirectory">The directory where logs will be stored.</param>
        /// <param name="stateObserver">An optional observer to update backup state (e.g., JSON file).</param>
        public EasySaveFacade(IBackupJobRepository jobRepository, string logDirectory, IBackupObserver? stateObserver = null)
        {
            // Create BackupManager with the repository and log directory
            _backupManager = new BackupManager(jobRepository, logDirectory);

            // Add the state observer if provided
            if (stateObserver != null)
            {
                _backupManager.AddObserver(stateObserver);
            }
        }

        /// <summary>
        /// Adds a new backup job.
        /// </summary>
        /// <param name="job">The backup job to be added.</param>
        public void AddJob(BackupJob job)
        {
            var cmd = new AddJobCommand(_backupManager, job);
            cmd.Execute();
        }

        /// <summary>
        /// Removes an existing backup job identified by its index.
        /// </summary>
        /// <param name="index">The zero-based index of the job to remove.</param>
        public void RemoveJob(int index)
        {
            var cmd = new RemoveJobCommand(_backupManager, index);
            cmd.Execute();
        }

        /// <summary>
        /// Updates an existing backup job by providing new values for its attributes.
        /// Parameters that are null or empty will not be updated.
        /// </summary>
        /// <param name="index">The zero-based index of the job to update.</param>
        /// <param name="newName">New name for the backup job (or null to keep the existing name).</param>
        /// <param name="newSource">New source directory (or null).</param>
        /// <param name="newTarget">New target directory (or null).</param>
        /// <param name="newType">New backup type (or null).</param>
        public void UpdateJob(int index, string? newName, string? newSource, string? newTarget, BackupType? newType)
        {
            var cmd = new UpdateJobCommand(_backupManager, index, newName, newSource, newTarget, newType);
            cmd.Execute();
        }

        /// <summary>
        /// Executes all configured backup jobs.
        /// </summary>
        public void ExecuteAllJobs()
        {
            var cmd = new ExecuteAllJobsCommand(_backupManager);
            cmd.Execute();
        }

        /// <summary>
        /// Executes a specific backup job identified by its index.
        /// </summary>
        /// <param name="index">The zero-based index of the job to execute.</param>
        public void ExecuteJobByIndex(int index)
        {
            var cmd = new ExecuteJobCommand(_backupManager, index);
            cmd.Execute();
        }

        /// <summary>
        /// Displays the list of existing backup jobs, including name, source, target, and type.
        /// </summary>
        public void ListJobs()
        {
            var cmd = new ListJobsCommand(_backupManager);
            cmd.Execute();
        }

        /// <summary>
        /// Returns the total number of configured backup jobs.
        /// </summary>
        /// <returns>The number of configured backup jobs.</returns>
        public int GetJobCount()
        {
            return _backupManager.GetBackupJobCount();
        }

        /// <summary>
        /// Returns the list of all backup jobs.
        /// </summary>
        /// <returns>A list of <see cref="BackupJob"/> objects.</returns>
        public List<BackupJob> ListBackupJobs()
        {
            return _backupManager.GetAllJobs(); // Assurez-vous que cette méthode existe dans BackupManager
        }

    }
}
