using EasySave.Core.Models;
using EasySaveLogs;
using EasySave.Core.Models.BackupStrategies;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;
using EasySave.Core.Template;
using Microsoft.Extensions.Configuration;

namespace EasySave.Core
{
    /// <summary>
    /// Manages backup jobs, including execution, addition, removal, and updates.
    /// Uses the Observer pattern to notify registered observers of state changes.
    /// Implements the Template Method pattern to execute backups with different strategies.
    /// </summary>
    public class BackupManager
    {
        /// <summary>
        /// Repository interface for storing and retrieving backup jobs.
        /// </summary>
        private readonly IBackupJobRepository _jobRepository;

        /// <summary>
        /// List of backup jobs managed by this instance.
        /// </summary>
        private readonly List<BackupJob> _backupJobs;

        /// <summary>
        /// Logger instance for recording backup activities.
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// List of registered observers monitoring backup state changes.
        /// </summary>
        private readonly List<IBackupObserver> _observers;

        private readonly int _maxJobs;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupManager"/> class.
        /// </summary>
        /// <param name="jobRepository">Repository instance for loading and saving backup jobs.</param>
        /// <param name="logDirectory">Path to the directory where logs will be stored.</param>
        public BackupManager(IBackupJobRepository jobRepository, string logDirectory, IConfiguration configuration)
        {
            _jobRepository = jobRepository;
            _logger = Logger.GetInstance(logDirectory);
            _backupJobs = _jobRepository.Load();
            _observers = new List<IBackupObserver>();

            // Charger MaxBackupJobs depuis appsettings.json (valeur par défaut : 5)
            _maxJobs = int.TryParse(configuration["MaxBackupJobs"], out int maxJobs) ? maxJobs : 5;
        }

        // ------------------------- Observer Methods -------------------------

        /// <summary>
        /// Adds an observer to monitor backup job state changes.
        /// </summary>
        /// <param name="observer">Observer instance to be added.</param>
        public void AddObserver(IBackupObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        /// <summary>
        /// Removes an observer from the list of registered observers.
        /// </summary>
        /// <param name="observer">Observer instance to be removed.</param>
        public void RemoveObserver(IBackupObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }

        /// <summary>
        /// Notifies all registered observers of a backup state change.
        /// </summary>
        /// <param name="state">The current state of the backup process.</param>
        private void NotifyObservers(BackupState state)
        {
            foreach (var obs in _observers)
            {
                obs.Update(state);
            }
        }

        /// <summary>
        /// Saves the current state of backup jobs to persistent storage.
        /// </summary>
        private void SaveChanges()
        {
            _jobRepository.Save(_backupJobs);
        }

        // ------------------------- Backup Job Management -------------------------

        /// <summary>
        /// Adds a new backup job to the list.
        /// </summary>
        /// <param name="job">The backup job to add.</param>
        /// <exception cref="InvalidOperationException">Thrown if the maximum number of jobs (5) is exceeded.</exception>
        public void AddBackupJob(BackupJob job)
        {
            if (_backupJobs.Count >= _maxJobs) 
            {
                throw new InvalidOperationException($"Maximum of {_maxJobs} backup jobs allowed.");
            }

            _backupJobs.Add(job);
            SaveChanges();
            Console.WriteLine($"✅ Backup job '{job.Name}' added successfully.");
        }

        /// <summary>
        /// Executes all configured backup jobs.
        /// </summary>
        public void ExecuteAllJobs()
        {
            foreach (var job in _backupJobs)
            {
                ExecuteBackup(job);
            }
        }

        /// <summary>
        /// Gets the total number of configured backup jobs.
        /// </summary>
        /// <returns>The number of backup jobs.</returns>
        public int GetBackupJobCount()
        {
            return _backupJobs.Count;
        }

        /// <summary>
        /// Executes a specific backup job by its index.
        /// </summary>
        /// <param name="index">Index of the backup job in the list.</param>
        public void ExecuteBackupByIndex(int index)
        {
            if (index < 0 || index >= _backupJobs.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range.");
        }

        /// <summary>
        /// Removes a backup job from the list.
        /// </summary>
        /// <param name="index">Index of the job to be removed.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the provided index is invalid.</exception>
        public void RemoveBackupJob(int index)
        {
            if (index < 0 || index >= _backupJobs.Count)
                throw new IndexOutOfRangeException($"No job at index {index}.");

            var jobToRemove = _backupJobs[index];
            _backupJobs.RemoveAt(index);
            SaveChanges();
            Console.WriteLine($"Backup job '{jobToRemove.Name}' removed.");
        }

        /// <summary>
        /// Updates an existing backup job with new details.
        /// Only non-null values will be updated.
        /// </summary>
        /// <param name="index">Index of the job to be updated.</param>
        /// <param name="newName">New name for the job (nullable).</param>
        /// <param name="newSource">New source directory (nullable).</param>
        /// <param name="newTarget">New target directory (nullable).</param>
        /// <param name="newType">New backup type (nullable).</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the provided index is invalid.</exception>
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

        /// <summary>
        /// Lists all configured backup jobs.
        /// </summary>
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

        // ------------------------- Backup Execution -------------------------

        /// <summary>
        /// Executes a backup job using the appropriate backup strategy.
        /// Implements the Template Method pattern.
        /// </summary>
        /// <param name="job">The backup job to execute.</param>
        private void ExecuteBackup(BackupJob job)
        {
            // Choose the appropriate backup strategy based on the backup type
            AbstractBackupAlgorithm algorithm = job.BackupType == BackupType.Complete
                ? new FullBackupAlgorithm(_logger, state => NotifyObservers(state), () => SaveChanges())
                : new DifferentialBackupAlgorithm(_logger, state => NotifyObservers(state), () => SaveChanges());
            try{
                 algorithm.Execute(job);
            }
            catch (Exception ex){
                Console.WriteLine($"Error executing backup job '{job.Name}': {ex.Message}");
            }
        }

        public List<BackupJob> GetAllJobs()
        {
            return _jobRepository.GetAllJobs();
        }

    }
}
