using EasySave.Core.Models;

namespace EasySave.Core.Repositories
{
    /// <summary>
    /// Interface defining persistence methods for backup jobs.
    /// </summary>
    public interface IBackupJobRepository
    {
        /// <summary>
        /// Loads the list of backup jobs from storage.
        /// </summary>
        /// <returns>A list of <see cref="BackupJob"/> instances.</returns>
        List<BackupJob> Load();

        /// <summary>
        /// Saves the list of backup jobs to storage.
        /// </summary>
        /// <param name="jobs">The list of backup jobs to be saved.</param>
        void Save(List<BackupJob> jobs);
        List<BackupJob> GetAllJobs();
    }
}
