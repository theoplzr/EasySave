using EasySaveApp.Models;
using EasySaveApp.Models.BackupStrategies;
using Newtonsoft.Json;

namespace EasySaveApp.Repositories
{
    /// <summary>
    /// Implémente IBackupJobRepository pour stocker les jobs dans un fichier JSON.
    /// </summary>
    public class JsonBackupJobRepository : IBackupJobRepository
    {
        private readonly string _filePath;

        public JsonBackupJobRepository(string filePath)
        {
            _filePath = filePath;
        }

        public List<BackupJob> Load()
        {
            if (!File.Exists(_filePath))
                return new List<BackupJob>();

            var json = File.ReadAllText(_filePath);
            var jobs = JsonConvert.DeserializeObject<List<BackupJob>>(json) ?? new List<BackupJob>();

            // Réinstancier la stratégie pour chaque job chargé, si besoin
            foreach (var job in jobs)
            {
                job._backupStrategy = BackupStrategyFactory.GetStrategy(job.BackupType);
            }

            return jobs;
        }

        public void Save(List<BackupJob> jobs)
        {
            var json = JsonConvert.SerializeObject(jobs, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}
