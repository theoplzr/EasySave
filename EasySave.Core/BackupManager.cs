using EasySave.Core.Models;
using EasySaveLogs;
using EasySave.Core.Models.BackupStrategies;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;
using EasySave.Core.Template;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using CryptoSoftLib;
using System.IO;
using System.Threading;

namespace EasySave.Core
{
    /// <summary>
    /// Manages backup jobs, observers, logging, and execution logic.
    /// </summary>
    public class BackupManager
    {
        private readonly IBackupJobRepository _jobRepository;
        private readonly List<BackupJob> _backupJobs;
        private readonly Logger _logger;
        private readonly List<IBackupObserver> _observers;
        private readonly int _maxJobs;
        private readonly string _cryptoSoftPath;
        private readonly string _businessSoftwareName;
        private readonly string[] _encryptionExtensions;
        private readonly string[] _priorityExtensions;
        private readonly string _encryptionKey;
        private readonly object _observersLock = new();
        private string _status = string.Empty; // Initialisation du statut

        // Seuil configurable en kilo-octets pour le transfert de gros fichiers
        private readonly int _transferThresholdInKiloBytes;
        // Verrou pour la synchronisation des transferts de gros fichiers
        private static readonly object _bigFileLock = new();
        private static bool _bigFileTransferInProgress = false;

        public BackupManager(IBackupJobRepository jobRepository, string logDirectory, IConfiguration configuration)
        {
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _backupJobs = _jobRepository.Load() ?? new List<BackupJob>();
            _observers = new List<IBackupObserver>();

            _maxJobs = int.TryParse(configuration["MaxBackupJobs"], out int maxJobs) ? maxJobs : 5;
            _cryptoSoftPath = "/Applications/CryptoSoft.app/Contents/MacOS/CryptoSoft";
            _businessSoftwareName = configuration["BusinessSoftware"] ?? "Spotify";
            _encryptionExtensions = configuration.GetSection("EncryptionExtensions").Get<string[]>() ?? Array.Empty<string>();
            _priorityExtensions = configuration.GetSection("PriorityExtensions").Get<string[]>() ?? Array.Empty<string>();
            _encryptionKey = "DefaultKey123";

            // Récupération du seuil de transfert en kilo-octets (n Ko)
            _transferThresholdInKiloBytes = int.TryParse(configuration["TransferThreshold"], out int threshold) ? threshold : 1024;

            // Initialize logger
            string logFormat = configuration["LogFormat"] ?? "JSON";
            _logger = Logger.GetInstance(logDirectory, logFormat);
        }

        // ------------------------- Gestion des Observateurs -------------------------

        public void AddObserver(IBackupObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void RemoveObserver(IBackupObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers(BackupState state)
        {
            lock (_observersLock)
            {
                foreach (var obs in _observers)
                {
                    obs.Update(state);
                }
            }
        }

        private void SaveChanges()
        {
            _jobRepository.Save(_backupJobs);
        }

        // ------------------------- Gestion des Jobs de Sauvegarde -------------------------

        public void AddBackupJob(BackupJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            _backupJobs.Add(job);
            SaveChanges();
            Console.WriteLine($"✅ Backup job '{job.Name}' added successfully.");
        }

        public async Task ExecuteAllJobsAsync()
        {
            // Lancer une seule tâche par job
            var tasks = _backupJobs.Select(job => Task.Run(() => ExecuteBackup(job)));
            await Task.WhenAll(tasks);
        }

        public int GetBackupJobCount()
        {
            return _backupJobs.Count;
        }

        public List<BackupJob> GetAllJobs()
        {
            return _backupJobs;
        }

        /// <summary>
        /// Exécute le job à l'index spécifié en une seule passe.
        /// </summary>
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

        /// <summary>
        /// Exécute le job en une seule passe, traitant d'abord les fichiers prioritaires puis les non-prioritaires.
        /// </summary>
        private async Task ExecuteBackup(BackupJob job)
        {
            bool alreadyLog = false;
            // Attendre tant que le logiciel métier est actif
            while (IsBusinessSoftwareRunning())
            {
                if (!alreadyLog)
                {
                    alreadyLog = true;
                    _logger.LogAction(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = job.Name,
                        Status = $"Pause: Business software '{_businessSoftwareName}' detected"
                    });
                    Console.WriteLine($"Backup job '{job.Name}' stopped: business software '{_businessSoftwareName}' is running.");
                }
                await Task.Delay(2000);
            }

            // Mise à jour de l'état de sauvegarde
            BackupState state = new BackupState
            {
                JobId = job.Id,
                BackupName = job.Name,
                Status = "En cours",
                LastActionTime = DateTime.Now,
                CurrentSourceFile = "En attente...",
                CurrentTargetFile = "En attente...",
                TotalFiles = Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length
            };

            NotifyObservers(state);

            // Choisir l’algorithme de sauvegarde en passant _businessSoftwareName au constructeur
            AbstractBackupAlgorithm algorithm = job.BackupType == BackupType.Complete
                ? new FullBackupAlgorithm(_logger, state => NotifyObservers(state), () => SaveChanges(), _businessSoftwareName)
                : new DifferentialBackupAlgorithm(_logger, state => NotifyObservers(state), () => SaveChanges(), _businessSoftwareName);

            try
            {
                algorithm.Execute(job);

                // Récupérer tous les fichiers dans le dossier cible
                var allFiles = Directory.GetFiles(job.TargetDirectory);
                // Séparer les fichiers prioritaires et non prioritaires
                var priorityFiles = allFiles.Where(file => _priorityExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).ToList();
                var normalFiles = allFiles.Except(priorityFiles).ToList();

                // Traiter d'abord les fichiers prioritaires
                foreach (var file in priorityFiles)
                {
                    SaveFile(file, job);
                }
                // Puis les fichiers non prioritaires
                foreach (var file in normalFiles)
                {
                    SaveFile(file, job);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'exécution de la sauvegarde : {ex.Message}");
            }
        }

        /// <summary>
        /// Sauvegarde et chiffre le fichier en fonction de sa taille.
        /// </summary>
        public void SaveFile(string file, BackupJob job)
        {
            var fileInfo = new FileInfo(file);
            var fileExtension = Path.GetExtension(file);

            // Vérifier si le fichier doit être crypté
            if (!_encryptionExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Fichier ignoré pour cryptage : {file}");
                return;
            }

            // Si le fichier dépasse le seuil, synchroniser le transfert
            if (fileInfo.Length > _transferThresholdInKiloBytes * 1024)
            {
                lock (_bigFileLock)
                {
                    while (_bigFileTransferInProgress)
                    {
                        Monitor.Wait(_bigFileLock);
                    }
                    _bigFileTransferInProgress = true;
                }

                try
                {
                    Console.WriteLine($"Chiffrement du fichier volumineux : {file}");
                    int encryptionTime = CryptoSoft.EncryptFile(file, _encryptionKey);
                    Console.WriteLine($"Fichier {file} crypté en {encryptionTime}ms");

                    _logger.LogAction(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = job.Name,
                        SourceFilePath = file,
                        TargetFilePath = file,
                        FileSize = fileInfo.Length,
                        TransferTimeMs = 0,
                        EncryptionTimeMs = encryptionTime,
                        Status = "Fichier crypté avec succès",
                        Level = Logger.LogLevel.Info
                    });
                }
                finally
                {
                    lock (_bigFileLock)
                    {
                        _bigFileTransferInProgress = false;
                        Monitor.PulseAll(_bigFileLock);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Chiffrement du fichier : {file}");
                int encryptionTime = CryptoSoft.EncryptFile(file, _encryptionKey);
                Console.WriteLine($"Fichier {file} crypté en {encryptionTime}ms");

                _logger.LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    SourceFilePath = file,
                    TargetFilePath = file,
                    FileSize = fileInfo.Length,
                    TransferTimeMs = 0,
                    EncryptionTimeMs = encryptionTime,
                    Status = "Fichier crypté avec succès",
                    Level = Logger.LogLevel.Info
                });
            }
        }

        /// <summary>
        /// Vérifie si le logiciel métier est en cours d'exécution.
        /// </summary>
        public bool IsBusinessSoftwareRunning()
        {
            return Process.GetProcessesByName(_businessSoftwareName).Any();
        }

        /// <summary>
        /// Retourne le statut actuel du backup manager.
        /// </summary>
        public string GetStatus()
        {
            return _status;
        }
    }
}
