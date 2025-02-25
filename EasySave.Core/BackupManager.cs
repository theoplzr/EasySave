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
using System.Linq;

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

        // Association de chaque job à son contrôleur (pour pause/play/stop)
        private readonly Dictionary<Guid, JobController> _jobControllers = new();

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
            // Créer et associer un contrôleur pour ce job
            _jobControllers[job.Id] = new JobController();

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
            // Supprimer le contrôleur associé
            _jobControllers.Remove(jobToRemove.Id);
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
        /// Intègre les contrôles de pause et d'arrêt.
        /// </summary>
        private async Task ExecuteBackup(BackupJob job)
        {
            // Récupérer le contrôleur associé au job
            if (!_jobControllers.TryGetValue(job.Id, out JobController controller))
            {
                controller = new JobController();
                _jobControllers[job.Id] = controller;
            }

            controller.State = JobState.Running;

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

            // Création d'un état initial avec les données réelles du job
            int totalFiles = Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length;
            BackupState state = new BackupState
            {
                JobId = job.Id,
                BackupName = job.Name,
                Status = "En cours",
                LastActionTime = DateTime.Now,
                CurrentSourceFile = "En attente...",
                CurrentTargetFile = "En attente...",
                TotalFiles = totalFiles,
                TotalSize = 0, // Calcul à implémenter si nécessaire
                RemainingFiles = totalFiles,
                RemainingSize = 0,
                ProgressPercentage = 0
            };

            NotifyObservers(state);

            // Choisir l’algorithme de sauvegarde en passant _businessSoftwareName au constructeur
            AbstractBackupAlgorithm algorithm = job.BackupType == BackupType.Complete
                ? new FullBackupAlgorithm(_logger, s => NotifyObservers(s), () => SaveChanges(), _businessSoftwareName)
                : new DifferentialBackupAlgorithm(_logger, s => NotifyObservers(s), () => SaveChanges(), _businessSoftwareName);

            try
            {
                algorithm.Execute(job);

                // Récupérer tous les fichiers dans le dossier cible
                var allFiles = Directory.GetFiles(job.TargetDirectory);
                var priorityFiles = allFiles.Where(file => _priorityExtensions.Any(ext => file.EndsWith(ext, System.StringComparison.OrdinalIgnoreCase))).ToList();
                var normalFiles = allFiles.Except(priorityFiles).ToList();
                int totalToProcess = priorityFiles.Count + normalFiles.Count;
                int processedFiles = 0;

                foreach (var file in priorityFiles.Concat(normalFiles))
                {
                    // Contrôle de pause et d'arrêt
                    controller.PauseEvent.Wait(controller.CancellationTokenSource.Token);
                    if (controller.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        Console.WriteLine($"Job '{job.Name}' stoppé.");
                        break;
                    }

                    SaveFile(file, job);
                    processedFiles++;

                    // Mise à jour de l'état avec les vraies données
                    state.LastActionTime = DateTime.Now;
                    state.CurrentSourceFile = file;
                    state.CurrentTargetFile = Path.Combine(job.TargetDirectory, Path.GetFileName(file));
                    state.RemainingFiles = totalToProcess - processedFiles;
                    state.ProgressPercentage = (int)((processedFiles / (double)totalToProcess) * 100);

                    // Si 100% atteint, définir le statut comme "Finished"
                    if(state.ProgressPercentage >= 100)
                    {
                        state.Status = "Finished";
                    }
                    else
                    {
                        state.Status = "Running";
                    }

                    state.Status = "Starting";
                    NotifyObservers(state);  
                    await Task.Delay(1000);  

                    state.Status = "Running";
                    NotifyObservers(state);

                    NotifyObservers(state);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'exécution de la sauvegarde : {ex.Message}");
            }
        }

        /// <summary>
        /// Notifie la progression d'un job via les observateurs.
        /// Ici, on se contente de récupérer les vraies données du job.
        /// </summary>
        private void NotifyProgress(Guid jobId, double progress)
        {
            // Récupérer le job correspondant pour obtenir le nom réel
            var job = _backupJobs.FirstOrDefault(j => j.Id == jobId);
            string backupName = job?.Name ?? "Unknown";

            BackupState state = new BackupState
            {
                JobId = jobId,
                BackupName = backupName,
                Status = "En cours",
                CurrentSourceFile = "En cours",
                CurrentTargetFile = "En cours",
                TotalFiles = job != null ? Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length : 0,
                TotalSize = 0,
                RemainingFiles = 0,
                RemainingSize = 0,
                ProgressPercentage = (int)progress
            };

            // Si la progression atteint 100%, on définit le statut comme "Terminé"
            if (state.ProgressPercentage >= 100)
            {
                state.Status = "Finished";
            }
            NotifyObservers(state);
        }

        /// <summary>
        /// Sauvegarde et chiffre le fichier en fonction de sa taille.
        /// </summary>
        public void SaveFile(string file, BackupJob job)
        {
            var fileInfo = new FileInfo(file);
            var fileExtension = Path.GetExtension(file);

            if (!_encryptionExtensions.Contains(fileExtension, System.StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Fichier ignoré pour cryptage : {file}");
                return;
            }

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
                    // Délai artificiel de 3 secondes pour simuler un temps de traitement long
                    System.Threading.Thread.Sleep(3000);

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
                System.Threading.Thread.Sleep(500);

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

        // ------------------------- Méthodes de contrôle (Pause, Resume, Stop) -------------------------

        /// <summary>
        /// Met en pause le job spécifié.
        /// </summary>
        public void PauseJob(Guid jobId)
        {
            if (_jobControllers.TryGetValue(jobId, out JobController controller) && controller.State == JobState.Running)
            {
                controller.State = JobState.Paused;
                controller.PauseEvent.Reset();
                _status = "Paused"; // Mise à jour du statut global
                Console.WriteLine($"Job {jobId} mis en pause."); // ✅ Ajout pour affichage console

                // Création d'un état mis à jour pour notifier l'UI
                var job = _backupJobs.FirstOrDefault(j => j.Id == jobId);
                var state = new BackupState
                {
                    JobId = jobId,
                    BackupName = job?.Name ?? "Unknown",
                    Status = "Paused",
                    LastActionTime = DateTime.Now,
                    CurrentSourceFile = "Paused",
                    CurrentTargetFile = "Paused",
                    TotalFiles = job != null ? Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length : 0,
                    RemainingFiles = 0,
                    TotalSize = 0,
                    RemainingSize = 0,
                    ProgressPercentage = 0
                };

                NotifyObservers(state);
            }
        }

        public void ResumeJob(Guid jobId)
        {
            if (_jobControllers.TryGetValue(jobId, out JobController controller) && controller.State == JobState.Paused)
            {
                controller.State = JobState.Running;
                controller.PauseEvent.Set();
                Console.WriteLine($"Job {jobId} repris.");

                var job = _backupJobs.FirstOrDefault(j => j.Id == jobId);
                var state = new BackupState
                {
                    JobId = jobId,
                    BackupName = job?.Name ?? "Unknown",
                    Status = "Running",
                    LastActionTime = DateTime.Now,
                    CurrentSourceFile = "Running",
                    CurrentTargetFile = "Running",
                    TotalFiles = job != null ? Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length : 0,
                    RemainingFiles = 0,
                    TotalSize = 0,
                    RemainingSize = 0,
                    ProgressPercentage = 0
                };

                NotifyObservers(state);
            }
        }

        public void StopJob(Guid jobId)
        {
            if (_jobControllers.TryGetValue(jobId, out JobController controller))
            {
                controller.State = JobState.Stopped;
                controller.CancellationTokenSource.Cancel();
                controller.PauseEvent.Set();
                Console.WriteLine($"Job {jobId} stoppé.");

                var job = _backupJobs.FirstOrDefault(j => j.Id == jobId);
                var state = new BackupState
                {
                    JobId = jobId,
                    BackupName = job?.Name ?? "Unknown",
                    Status = "Stopped",
                    LastActionTime = DateTime.Now,
                    CurrentSourceFile = "Stopped",
                    CurrentTargetFile = "Stopped",
                    TotalFiles = job != null ? Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length : 0,
                    RemainingFiles = 0,
                    TotalSize = 0,
                    RemainingSize = 0,
                    ProgressPercentage = 0
                };

                NotifyObservers(state);
            }
        }
    }
}