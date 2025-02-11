using EasySave.Core.Models;
using EasySaveLogs;
using EasySave.Core.Models.BackupStrategies;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;
using EasySave.Core.Template;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace EasySave.Core
{
    /// <summary>
    /// Manages backup jobs, including execution, addition, removal, and updates.
    /// Uses the Observer pattern to notify registered observers of state changes.
    /// Implements the Template Method pattern to execute backups with different strategies.
    /// </summary>
    public class BackupManager
    {
        private readonly IBackupJobRepository _jobRepository;
        private readonly List<BackupJob> _backupJobs;
        private readonly Logger _logger;
        private readonly List<IBackupObserver> _observers;
        private readonly int _maxJobs;
        private readonly string _cryptoSoftPath = "/Applications/CryptoSoft.app/Contents/MacOS/CryptoSoft"; 
        private readonly string _businessSoftwareName = "Calculator"; 

        public BackupManager(IBackupJobRepository jobRepository, string logDirectory, IConfiguration configuration)
        {
            _jobRepository = jobRepository;
            _backupJobs = _jobRepository.Load();
            _observers = new List<IBackupObserver>();

            // Charger LogFormat et MaxBackupJobs depuis la configuration
            string logFormat = configuration["Logging:LogFormat"] ?? "JSON";
            _maxJobs = int.TryParse(configuration["MaxBackupJobs"], out int maxJobs) ? maxJobs : 5;

            // Initialiser le logger avec le format correct
            _logger = Logger.GetInstance(logDirectory, logFormat);
        }

        // ------------------------- Observer Methods -------------------------

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

        // ------------------------- Backup Job Management -------------------------

        public void AddBackupJob(BackupJob job)
        {
            _backupJobs.Add(job);
            SaveChanges();
            Console.WriteLine($"✅ Backup job '{job.Name}' added successfully.");
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
            if (IsBusinessSoftwareRunning())
            {
                Console.WriteLine("⚠️ Un logiciel métier est en cours d'exécution. Sauvegarde interdite !");
                return;
            }

            if (index < 0 || index >= _backupJobs.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range.");

            ExecuteBackup(_backupJobs[index]);
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

        private void ExecuteBackup(BackupJob job)
        {
                // Récupérez le nom du logiciel métier depuis la configuration
                var businessSoftware = ConfigurationProvider.BusinessSoftware; // par exemple "Calculator"
                if (!string.IsNullOrEmpty(businessSoftware) && Services.BusinessSoftwareChecker.IsBusinessSoftwareRunning(businessSoftware))
                {
                    // Loggez l’arrêt et affichez un message
                    _logger.LogAction(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = job.Name,
                        SourceFilePath = "",
                        TargetFilePath = "",
                        FileSize = 0,
                        TransferTimeMs = 0,
                        EncryptionTimeMs = 0,
                        Status = $"Stopped: Business software '{businessSoftware}' detected"
                    });
                    Console.WriteLine($"Backup job '{job.Name}' stopped: business software '{businessSoftware}' is running.");
                    return; // Interrompt l’exécution
                }

                // Choisir l’algorithme de sauvegarde approprié
                AbstractBackupAlgorithm algorithm = job.BackupType == BackupType.Complete
                    ? new FullBackupAlgorithm(_logger, state => NotifyObservers(state), () => SaveChanges())
                    : new DifferentialBackupAlgorithm(_logger, state => NotifyObservers(state), () => SaveChanges());

                try
                {
                    algorithm.Execute(job);
                }
                catch (OperationCanceledException ex)
    {
        // Cas où on a détecté le logiciel métier en plein milieu
        Console.WriteLine($"⚠️ Backup '{job.Name}' interrupted: {ex.Message}");
        _logger.LogAction(new LogEntry
        {
            Timestamp = DateTime.Now,
            BackupName = job.Name,
            SourceFilePath = "",
            TargetFilePath = "",
            FileSize = 0,
            TransferTimeMs = 0,
            EncryptionTimeMs = 0,
            Status = $"Interrupted in mid-backup: {ex.Message}",
            Level = Logger.LogLevel.Warning
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error executing backup job '{job.Name}': {ex.Message}");
    }
        }

        /// <summary>
        /// Vérifie si un logiciel métier est en cours d'exécution.
        /// </summary>
        public bool IsBusinessSoftwareRunning()
        {
            return Process.GetProcessesByName(_businessSoftwareName).Any();
        }

        /// <summary>
        /// Crypte un fichier avec CryptoSoft et mesure le temps de cryptage.
        /// </summary>
        public long EncryptFile(string filePath)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = _cryptoSoftPath,
                    Arguments = filePath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du cryptage : {ex.Message}");
                return -1;
            }
        }

        public List<BackupJob> GetAllJobs()
        {
            return _backupJobs;
        }
    }
}
