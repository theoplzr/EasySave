using EasySave.Core.Models;
using EasySave.Core.Models.BackupStrategies;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace EasySave.Core.Repositories
{
    /// <summary>
    /// Stores and retrieves <see cref="BackupJob"/> objects using a JSON file.
    /// Implements <see cref="IBackupJobRepository"/>.
    /// </summary>
    public class JsonBackupJobRepository : IBackupJobRepository
    {
        /// <summary>
        /// The file path where backup jobs are persisted.
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

            // Reinitialize the backup strategy for each loaded job
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

        /// <summary>
        /// Retrieves all backup jobs from the storage.
        /// </summary>
        /// <returns>A list of <see cref="BackupJob"/> instances.</returns>
        public List<BackupJob> GetAllJobs()
        {
            return Load();
        }
    }
}
