using EasySaveApp.Models;
using EasySaveApp.Models.BackupStrategies;
using Newtonsoft.Json;

namespace EasySaveApp.Repositories
{
    /// <summary>
    /// Implements <see cref="IBackupJobRepository"/> to store backup jobs in a JSON file.
    /// </summary>
    public class JsonBackupJobRepository : IBackupJobRepository
    {
        /// <summary>
        /// The file path where backup jobs are stored.
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonBackupJobRepository"/> class.
        /// </summary>
        /// <param name="filePath">The path of the JSON file used for storage.</param>
        public JsonBackupJobRepository(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Loads the list of backup jobs from the JSON file.
        /// </summary>
        /// <returns>A list of <see cref="BackupJob"/> instances.</returns>
        public List<BackupJob> Load()
        {
            if (!File.Exists(_filePath))
                return new List<BackupJob>();

            var json = File.ReadAllText(_filePath);
            var jobs = JsonConvert.DeserializeObject<List<BackupJob>>(json) ?? new List<BackupJob>();

            // Reinstantiate the backup strategy for each loaded job if needed
            foreach (var job in jobs)
            {
                job._backupStrategy = BackupStrategyFactory.GetStrategy(job.BackupType);
            }

            return jobs;
        }

        /// <summary>
        /// Saves the list of backup jobs to the JSON file.
        /// </summary>
        /// <param name="jobs">The list of backup jobs to be saved.</param>
        public void Save(List<BackupJob> jobs)
        {
            var json = JsonConvert.SerializeObject(jobs, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}
