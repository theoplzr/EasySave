using System.Diagnostics;
using EasySaveApp.Models;
using EasySaveLogs;
using Newtonsoft.Json;
using EasySaveApp.Models.BackupStrategies;

namespace EasySaveApp
{
    public class BackupManager
    {
        private readonly List<BackupJob> _backupJobs;
        private readonly Logger _logger;
        private const string StateFilePath = "state.json";
        private const string BackupJobsFilePath = "backup_jobs.json";

        public BackupManager(string logDirectory)
        {
            _logger = new Logger(logDirectory);
            _backupJobs = LoadBackupJobs();
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
                        continue;
                    }
                    catch (IOException ioEx)
                    {
                        Console.WriteLine($"Erreur lors de la copie du fichier {file}: {ioEx.Message}");
                        continue;
                    }
                    stopwatch.Stop();

                    var fileSize = new FileInfo(file).Length;
                    filesProcessed++;
                    bytesProcessed += fileSize;

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

                    var state = new BackupState
                    {
                        JobId = job.Id,
                        BackupName = job.Name,
                        LastActionTime = DateTime.Now,
                        Status = "Actif",
                        TotalFiles = files.Length,
                        TotalSize = files.Sum(f => new FileInfo(f).Length),
                        RemainingFiles = files.Length - filesProcessed,
                        RemainingSize = files.Sum(f => new FileInfo(f).Length) - bytesProcessed,
                        CurrentSourceFile = file,
                        CurrentTargetFile = targetFilePath
                    };

                    File.WriteAllText(StateFilePath, JsonConvert.SerializeObject(state, Formatting.Indented));
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