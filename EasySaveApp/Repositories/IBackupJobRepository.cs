using EasySaveApp.Models;

namespace EasySaveApp.Repositories
{
    /// <summary>
    /// Interface définissant les méthodes de persistance pour les BackupJobs.
    /// </summary>
    public interface IBackupJobRepository
    {
        /// <summary>
        /// Charge la liste des BackupJobs.
        /// </summary>
        List<BackupJob> Load();

        /// <summary>
        /// Sauvegarde la liste des BackupJobs.
        /// </summary>
        /// <param name="jobs">La liste des jobs à sauvegarder.</param>
        void Save(List<BackupJob> jobs);
    }
}
