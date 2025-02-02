using System.Diagnostics;
using EasySaveApp.Models;
using EasySaveLogs;
using Newtonsoft.Json;
using EasySaveApp.Models.BackupStrategies;
using EasySaveApp.Observers;

namespace EasySaveApp
{
    public class BackupManager
    {
        private readonly List<BackupJob> _backupJobs;
        private readonly Logger _logger;

        // Liste d'observers pour l'état des sauvegardes
        private readonly List<IBackupObserver> _observers;

        private const string BackupJobsFilePath = "backup_jobs.json";

        public BackupManager(string logDirectory)
        {
            _logger = new Logger(logDirectory);
            _backupJobs = LoadBackupJobs();
            _observers = new List<IBackupObserver>();
        }

        // Méthodes pour s'abonner/désabonner aux notifications
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

        // Méthode de notification
        private void NotifyObservers(BackupState state)
        {
            foreach (var observer in _observers)
            {
                observer.Update(state);
            }
        }

        private List<BackupJob> LoadBackupJobs()
        {
            if (File.Exists(BackupJobsFilePath))
            {
                var json = File.ReadAllText(BackupJobsFilePath);
                var jobs = JsonConvert.DeserializeObject<List<BackupJob>>(json) ?? new List<BackupJob>();
                
                // Réinstancier _backupStrategy pour chaque job chargé
                foreach (var job in jobs)
                {
                    job._backupStrategy = BackupStrategyFactory.GetStrategy(job.BackupType);
                }
                
                return jobs;
            }
            return new List<BackupJob>();
        }

        private void SaveBackupJobs()
        {
            File.WriteAllText(BackupJobsFilePath, JsonConvert.SerializeObject(_backupJobs, Formatting.Indented));
        }

        public void AddBackupJob(BackupJob job)
        {
            if (_backupJobs.Count >= 5)
            {
                throw new InvalidOperationException("Maximum of 5 backup jobs allowed.");
            }

            _backupJobs.Add(job);
            SaveBackupJobs();
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

        // NEW: Supprimer un job par son index (zéro-based)
        public void RemoveBackupJob(int index)
        {
            if (index < 0 || index >= _backupJobs.Count)
                throw new IndexOutOfRangeException($"No job at index {index}.");

            var jobToRemove = _backupJobs[index];
            _backupJobs.RemoveAt(index);
            SaveBackupJobs();
            Console.WriteLine($"Backup job '{jobToRemove.Name}' removed.");
        }

        // NEW: Mettre à jour un job
        // Les paramètres non null ou non vides sont pris en compte, sinon on garde l'ancienne valeur.
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

            SaveBackupJobs();
            Console.WriteLine($"Job '{job.Name}' updated successfully.");
        }

        // NEW: Lister les jobs
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
            Console.WriteLine($"Executing backup: {job.Name}");
            var stopwatch = new Stopwatch();

            try
            {
                if (!Directory.Exists(job.SourceDirectory))
                {
                    Console.WriteLine($"Source directory does not exist: {job.SourceDirectory}");
                    return;
                }

                if (!Directory.Exists(job.TargetDirectory))
                {
                    if (string.IsNullOrEmpty(job.TargetDirectory))
                    {
                        Console.WriteLine("Error: Target directory is null or empty.");
                        return;
                    }
                    Directory.CreateDirectory(job.TargetDirectory);
                }

                var files = Directory.GetFiles(job.SourceDirectory, "*.*", SearchOption.AllDirectories);

                // Calculer la taille totale et le nombre total de fichiers
                long totalSize = files.Sum(f => new FileInfo(f).Length);
                int totalFiles = files.Length;

                int filesProcessed = 0;
                long bytesProcessed = 0;
                var backupStrategy = BackupStrategyFactory.GetStrategy(job.BackupType);

                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(job.SourceDirectory, file);
                    var targetFilePath = Path.Combine(job.TargetDirectory, relativePath);
                    var targetDirectory = Path.GetDirectoryName(targetFilePath);
                    
                    if (!Directory.Exists(targetDirectory) && !string.IsNullOrEmpty(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    if (!backupStrategy.ShouldCopyFile(file, targetFilePath))
                    {
                        continue;
                    }

                    stopwatch.Restart();
                    try
                    {
                        File.Copy(file, targetFilePath, true);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Error: accès refusé pour le fichier : {file}");
                        // Log erreur
                        _logger.LogAction(new LogEntry
                        {
                            Timestamp = DateTime.Now,
                            BackupName = job.Name,
                            SourceFilePath = file,
                            TargetFilePath = targetFilePath,
                            FileSize = 0,
                            TransferTimeMs = -1,
                            Status = "AccessDenied"
                        });
                        continue;
                    }
                    catch (IOException ioEx)
                    {
                        Console.WriteLine($"Erreur lors de la copie du fichier {file}: {ioEx.Message}");
                        // Log erreur
                        _logger.LogAction(new LogEntry
                        {
                            Timestamp = DateTime.Now,
                            BackupName = job.Name,
                            SourceFilePath = file,
                            TargetFilePath = targetFilePath,
                            FileSize = 0,
                            TransferTimeMs = -1,
                            Status = "IOError"
                        });
                        continue;
                    }
                    stopwatch.Stop();

                    var fileSize = new FileInfo(file).Length;
                    filesProcessed++;
                    bytesProcessed += fileSize;

                    // Log succès
                    _logger.LogAction(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = job.Name,
                        SourceFilePath = file,
                        TargetFilePath = targetFilePath,
                        FileSize = fileSize,
                        TransferTimeMs = stopwatch.ElapsedMilliseconds,
                        Status = "Success"
                    });

                    Console.WriteLine($"Copied: {file} -> {targetFilePath}");

                    // Créer un nouvel état à chaque fichier copié
                    var state = new BackupState
                    {
                        JobId = job.Id,
                        BackupName = job.Name,
                        LastActionTime = DateTime.Now,
                        Status = "Actif",
                        TotalFiles = totalFiles,
                        TotalSize = totalSize,
                        RemainingFiles = totalFiles - filesProcessed,
                        RemainingSize = totalSize - bytesProcessed,
                        CurrentSourceFile = file,
                        CurrentTargetFile = targetFilePath
                    };

                    // Notifier tous les observers
                    NotifyObservers(state);
                }

                Console.WriteLine($"Backup '{job.Name}' completed successfully.");
                SaveBackupJobs();
            }
            catch (Exception ex)
            {
                _logger.LogAction(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    Status = $"Error: {ex.Message}"
                });

                Console.WriteLine($"Error during backup '{job.Name}': {ex.Message}");
            }
        }
    }
}
