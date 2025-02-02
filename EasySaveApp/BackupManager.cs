using EasySaveApp.Models;
using EasySaveLogs;
using EasySaveApp.Models.BackupStrategies;
using EasySaveApp.Observers;
using EasySaveApp.Repositories;
using EasySaveApp.Template;

namespace EasySaveApp
{
    public class BackupManager
    {
        private readonly IBackupJobRepository _jobRepository; 
        private readonly List<BackupJob> _backupJobs;
        private readonly Logger _logger;

        private readonly List<IBackupObserver> _observers;

        public BackupManager(IBackupJobRepository jobRepository, string logDirectory)
        {
            _jobRepository = jobRepository;
            _logger = Logger.GetInstance(logDirectory)

            // Charger les jobs depuis le repository
            _backupJobs = _jobRepository.Load();

            // Initialiser la liste d'observers
            _observers = new List<IBackupObserver>();
        }

        // Méthodes d'observers
        public void AddObserver(IBackupObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void RemoveObserver(IBackupObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }

        private void NotifyObservers(BackupState state)
        {
            foreach (var obs in _observers)
            {
                obs.Update(state);
            }
        }

        private void SaveChanges()
        {
            _jobRepository.Save(_backupJobs);
        }

        public void AddBackupJob(BackupJob job)
        {
            if (_backupJobs.Count >= 5)
            {
                throw new InvalidOperationException("Maximum of 5 backup jobs allowed.");
            }

            _backupJobs.Add(job);
            SaveChanges();
            Console.WriteLine($"Backup job '{job.Name}' added.");
        }

        public void ExecuteAllJobs()
        {
            foreach (var job in _backupJobs)
            {
                ExecuteBackup(job);
            }
        }

        public int GetBackupJobCount()
        {
            return _backupJobs.Count;
        }

        public void ExecuteBackupByIndex(int index)
        {
            if (index >= 0 && index < _backupJobs.Count)
            {
                ExecuteBackup(_backupJobs[index]);
            }
            else
            {
                Console.WriteLine($"⚠ Erreur : Index {index + 1} hors de portée.");
            }
        }

        public void RemoveBackupJob(int index)
        {
            if (index < 0 || index >= _backupJobs.Count)
                throw new IndexOutOfRangeException($"No job at index {index}.");

            var jobToRemove = _backupJobs[index];
            _backupJobs.RemoveAt(index);
            SaveChanges();
            Console.WriteLine($"Backup job '{jobToRemove.Name}' removed.");
        }

        public void UpdateBackupJob(int index, string? newName, string? newSource, string? newTarget, BackupType? newType)
        {
            if (index < 0 || index >= _backupJobs.Count)
                throw new IndexOutOfRangeException($"No job at index {index}.");

            var job = _backupJobs[index];

            if (!string.IsNullOrEmpty(newName))
                job.Name = newName;

            if (!string.IsNullOrEmpty(newSource))
                job.SourceDirectory = newSource;

            if (!string.IsNullOrEmpty(newTarget))
                job.TargetDirectory = newTarget;

            if (newType.HasValue)
            {
                job.BackupType = newType.Value;
                job._backupStrategy = BackupStrategyFactory.GetStrategy(newType.Value);
            }

            SaveChanges();
            Console.WriteLine($"Job '{job.Name}' updated successfully.");
        }

        public void ListBackupJobs()
        {
            if (_backupJobs.Count == 0)
            {
                Console.WriteLine("No backup jobs configured.");
                return;
            }

            for (int i = 0; i < _backupJobs.Count; i++)
            {
                var job = _backupJobs[i];
                Console.WriteLine($"[{i + 1}] Name: {job.Name}, Source: {job.SourceDirectory}, Target: {job.TargetDirectory}, Type: {job.BackupType}");
            }
        }

        /// <summary>
        /// Méthode refactorisée pour utiliser le Template Method :
        /// On instancie la sous-classe adaptée (Full vs Differential),
        /// puis on appelle algorithm.Execute(job).
        /// </summary>
        private void ExecuteBackup(BackupJob job)
        {
            // Choix de l'implémentation du Template Method en fonction du type de sauvegarde
            AbstractBackupAlgorithm algorithm;
            if (job.BackupType == BackupType.Complete)
            {
                algorithm = new FullBackupAlgorithm(
                    _logger,
                    state => NotifyObservers(state),  
                    () => SaveChanges()               
                );
            }
            else
            {
                algorithm = new DifferentialBackupAlgorithm
                (
                    _logger,
                    state => NotifyObservers(state),
                    () => SaveChanges()
                );
            }

            // Appel de la méthode template
            algorithm.Execute(job);
        }
    }
}
