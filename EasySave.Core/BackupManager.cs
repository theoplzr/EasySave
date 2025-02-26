using EasySave.Core.Models;
using EasySave.Core.Models.BackupStrategies;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;
using EasySave.Core.Template;
using EasySaveLogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using CryptoSoftLib;

namespace EasySave.Core
{
    /// <summary>
    /// Manages backup jobs, observers, logging, and execution logic.
    /// Provides methods for job creation, removal, and control (pause, resume, stop).
    /// </summary>
    public class BackupManager
    {
        #region Fields

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
        private string _status = string.Empty;

        // Associates each job with its controller for pause/play/stop.
        private readonly Dictionary<Guid, JobController> _jobControllers = new();

        // A configurable threshold in kilobytes for transferring large files.
        private readonly int _transferThresholdInKiloBytes;

        // A lock to synchronize large file transfers.
        private static readonly object _bigFileLock = new();
        private static bool _bigFileTransferInProgress = false;

        private const double NetworkThresholdMBps = 10.0; // Network usage threshold in MB/s
        private const int MaxParallelJobs = 5;            // Maximum number of parallel jobs
        private const int MinParallelJobs = 1;            // Minimum number of parallel jobs
        private int _currentParallelJobs = MaxParallelJobs;
        private readonly SemaphoreSlim _jobLimiter;       // Semaphore to limit the parallel job count

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupManager"/> class.
        /// </summary>
        /// <param name="jobRepository">Repository for loading and saving backup jobs.</param>
        /// <param name="logDirectory">Path to the directory where logs are stored.</param>
        /// <param name="configuration">Configuration object used to get custom settings (e.g., max jobs).</param>
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

            // Retrieve the threshold in kilobytes for large file transfers (default 1024).
            _transferThresholdInKiloBytes = int.TryParse(configuration["TransferThreshold"], out int threshold) 
                ? threshold 
                : 1024;

            // Initialize the parallel job semaphore.
            _jobLimiter = new SemaphoreSlim(_currentParallelJobs);

            // Initialize the logger.
            string logFormat = configuration["LogFormat"] ?? "JSON";
            _logger = Logger.GetInstance(logDirectory, logFormat);
        }

        #endregion

        #region Observer Management

        /// <summary>
        /// Adds an observer to the list of observers, if it is not already present.
        /// </summary>
        /// <param name="observer">An <see cref="IBackupObserver"/> to be added.</param>
        public void AddObserver(IBackupObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        /// <summary>
        /// Removes a given observer from the list of observers.
        /// </summary>
        /// <param name="observer">An <see cref="IBackupObserver"/> to be removed.</param>
        public void RemoveObserver(IBackupObserver observer)
        {
            _observers.Remove(observer);
        }

        /// <summary>
        /// Notifies all registered observers of a given backup state.
        /// </summary>
        /// <param name="state">The <see cref="BackupState"/> containing updated info.</param>
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

        #endregion

        #region Backup Job Management

        /// <summary>
        /// Adds a new backup job to the manager.
        /// </summary>
        /// <param name="job">The <see cref="BackupJob"/> to be added.</param>
        /// <exception cref="ArgumentNullException">Thrown if the job is null.</exception>
        public void AddBackupJob(BackupJob job)
        {
            if (job == null) 
                throw new ArgumentNullException(nameof(job));

            _backupJobs.Add(job);

            // Create and associate a controller for this job.
            _jobControllers[job.Id] = new JobController();

            SaveChanges();
            Console.WriteLine($"✅ Backup job '{job.Name}' added successfully.");
        }

        /// <summary>
        /// Removes the backup job located at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the job to remove.</param>
        public void RemoveBackupJob(int index)
        {
            if (index < 0 || index >= _backupJobs.Count)
                throw new IndexOutOfRangeException($"No job at index {index}.");

            var jobToRemove = _backupJobs[index];
            _backupJobs.RemoveAt(index);

            // Remove the associated controller.
            _jobControllers.Remove(jobToRemove.Id);

            SaveChanges();
            Console.WriteLine($"Backup job '{jobToRemove.Name}' removed.");
        }

        /// <summary>
        /// Updates properties of a backup job, reloading strategy if necessary.
        /// </summary>
        /// <param name="index">The zero-based index of the job to update.</param>
        /// <param name="newName">The new name, if any.</param>
        /// <param name="newSource">A new source directory, if any.</param>
        /// <param name="newTarget">A new target directory, if any.</param>
        /// <param name="newType">A new <see cref="BackupType"/>, if any.</param>
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
        /// Prints a list of all configured backup jobs to the console.
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

        /// <summary>
        /// Retrieves the total number of configured backup jobs.
        /// </summary>
        /// <returns>An integer representing the job count.</returns>
        public int GetBackupJobCount()
        {
            return _backupJobs.Count;
        }

        /// <summary>
        /// Retrieves the list of all backup jobs currently managed.
        /// </summary>
        /// <returns>A <see cref="List{BackupJob}"/> containing all jobs.</returns>
        public List<BackupJob> GetAllJobs()
        {
            return _backupJobs;
        }

        #endregion

        #region Execution Methods

        /// <summary>
        /// Executes all backup jobs asynchronously, respecting a semaphore limit for parallel jobs.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExecuteAllJobsAsync()
        {
            var tasks = _backupJobs.Select(async job =>
            {
                await _jobLimiter.WaitAsync();
                try
                {
                    await ExecuteBackup(job);
                }
                finally
                {
                    _jobLimiter.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Executes a single backup job by its list index in a one-pass manner.
        /// </summary>
        /// <param name="index">The zero-based index of the job to execute.</param>
        public void ExecuteBackupByIndex(int index)
        {
            if (IsBusinessSoftwareRunning())
            {
                Console.WriteLine("⚠️ A business software is running. Backups are disallowed at this time!");
                return;
            }

            if (index < 0 || index >= _backupJobs.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range.");

            ExecuteBackup(_backupJobs[index]); // Fire-and-forget approach
        }

        /// <summary>
        /// Executes the specified backup job in one pass, first processing high-priority files,
        /// then the rest, and handling pause/stop requests if necessary.
        /// Dynamically adjusts the number of parallel jobs based on network usage.
        /// </summary>
        /// <param name="job">The <see cref="BackupJob"/> to execute.</param>
        private async Task ExecuteBackup(BackupJob job)
        {
            double networkUsage = GetNetworkUsage();
            Console.WriteLine($"📡 Current network usage: {networkUsage} MBps");
            Console.WriteLine($"🔄 Current parallel jobs: {_currentParallelJobs}");

            // Dynamically adjust parallel jobs based on network load
            if (networkUsage > NetworkThresholdMBps && _currentParallelJobs > MinParallelJobs)
            {
                _currentParallelJobs = Math.Max(_currentParallelJobs - 1, MinParallelJobs);
                _jobLimiter.Wait(1); // Reduce the number of parallel jobs
                Console.WriteLine($"⚠️ Reduced parallel jobs to: {_currentParallelJobs}");
            }
            else if (networkUsage < NetworkThresholdMBps / 2 && _currentParallelJobs < MaxParallelJobs)
            {
                _currentParallelJobs = Math.Min(_currentParallelJobs + 1, MaxParallelJobs);
                _jobLimiter.Release(1); // Increase the number of parallel jobs
                Console.WriteLine($"✅ Increased parallel jobs to: {_currentParallelJobs}");
            }

            // Retrieve or create a JobController for this job
            if (!_jobControllers.TryGetValue(job.Id, out JobController controller))
            {
                controller = new JobController();
                _jobControllers[job.Id] = controller;
            }
            controller.State = JobState.Running;

            // Pause if the business software is detected
            bool alreadyLog = false;
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
                    Console.WriteLine($"Backup job '{job.Name}' paused because '{_businessSoftwareName}' is running.");
                }
                await Task.Delay(2000);
            }

            // Initialize backup state
            int totalFiles = Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length;
            var state = new BackupState
            {
                JobId = job.Id,
                BackupName = job.Name,
                Status = "Running",
                LastActionTime = DateTime.Now,
                CurrentSourceFile = "Waiting...",
                CurrentTargetFile = "Waiting...",
                TotalFiles = totalFiles,
                TotalSize = 0,
                RemainingFiles = totalFiles,
                RemainingSize = 0,
                ProgressPercentage = 0
            };
            NotifyObservers(state);

            // Choose the backup algorithm (Full or Differential)
            AbstractBackupAlgorithm algorithm = job.BackupType == BackupType.Complete
                ? new FullBackupAlgorithm(_logger, s => NotifyObservers(s), () => SaveChanges(), _businessSoftwareName)
                : new DifferentialBackupAlgorithm(_logger, s => NotifyObservers(s), () => SaveChanges(), _businessSoftwareName);

            try
            {
                // Execute the initial copy algorithm
                algorithm.Execute(job);

                // Retrieve all files from the target directory
                var allFiles = Directory.GetFiles(job.TargetDirectory);

                var priorityFiles = allFiles
                    .Where(file => _priorityExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                var normalFiles = allFiles.Except(priorityFiles).ToList();

                int totalToProcess = priorityFiles.Count + normalFiles.Count;
                int processedFiles = 0;

                // Process priority files first, then the remaining files
                foreach (var file in priorityFiles.Concat(normalFiles))
                {
                    // Handle pause/stop requests
                    controller.PauseEvent.Wait(controller.CancellationTokenSource.Token);
                    if (controller.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        Console.WriteLine($"Job '{job.Name}' stopped by request.");
                        state.Status = "Stopped";
                        NotifyObservers(state);
                        return;
                    }

                    // Encrypt or handle the file according to size thresholds
                    SaveFile(file, job);
                    processedFiles++;

                    // Update state
                    state.LastActionTime = DateTime.Now;
                    state.CurrentSourceFile = file;
                    state.CurrentTargetFile = Path.Combine(job.TargetDirectory, Path.GetFileName(file));
                    state.RemainingFiles = totalToProcess - processedFiles;
                    state.ProgressPercentage = (int)((processedFiles / (double)totalToProcess) * 100);

                    // Mark the job as finished if we've hit 100%
                    if (state.ProgressPercentage >= 100)
                    {
                        state.Status = "Finished";
                    }
                    else
                    {
                        state.Status = "Running";
                    }

                    NotifyObservers(state);

                    // Short delay for demonstration purposes
                    await Task.Delay(1000);
                }

                // Final notification of completion
                state.Status = "Finished";
                state.ProgressPercentage = 100;
                NotifyObservers(state);

                Console.WriteLine($"✅ Backup job '{job.Name}' completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during backup execution: {ex.Message}");
                state.Status = "Error";
                NotifyObservers(state);
            }
        }

        /// <summary>
        /// Encrypts the file if it matches encryption settings, respecting a named mutex for concurrency control.
        /// </summary>
        /// <param name="file">The path of the file to encrypt.</param>
        /// <param name="job">The associated <see cref="BackupJob"/>.</param>
        public void SaveFile(string file, BackupJob job)
        {
            var fileInfo = new FileInfo(file);
            var fileExtension = Path.GetExtension(file);

            // If the file's extension is not in the encryption list, skip it
            if (!_encryptionExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Skipped encryption for file: {file}");
                return;
            }

            using (Mutex mutex = new Mutex(false, "CryptoSoft_Mutex"))
            {
                // Wait up to 5 seconds to acquire the mutex
                if (!mutex.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    Console.WriteLine($"CryptoSoft is already running. Skipped: {file}");
                    return;
                }

                try
                {
                    Console.WriteLine($"Encrypting file: {file}");
                    int encryptionTime = CryptoSoft.EncryptFile(file, _encryptionKey);

                    Console.WriteLine($"File {file} encrypted in {encryptionTime}ms");

                    _logger.LogAction(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = job.Name,
                        SourceFilePath = file,
                        TargetFilePath = file,
                        FileSize = fileInfo.Length,
                        TransferTimeMs = 0,
                        EncryptionTimeMs = encryptionTime,
                        Status = "File successfully encrypted",
                        Level = Logger.LogLevel.Info
                    });
                }
                finally
                {
                    mutex.ReleaseMutex(); // Always release after encryption
                }
            }
        }

        #endregion

        #region Job Control Methods (Pause, Resume, Stop)

        /// <summary>
        /// Pauses the job with the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job to pause.</param>
        public void PauseJob(Guid jobId)
        {
            if (_jobControllers.TryGetValue(jobId, out JobController controller) 
                && controller.State == JobState.Running)
            {
                controller.State = JobState.Paused;
                controller.PauseEvent.Reset();
                _status = "Paused";
                Console.WriteLine($"Job {jobId} paused.");

                var job = _backupJobs.FirstOrDefault(j => j.Id == jobId);
                var state = new BackupState
                {
                    JobId = jobId,
                    BackupName = job?.Name ?? "Unknown",
                    Status = "Paused",
                    LastActionTime = DateTime.Now,
                    CurrentSourceFile = "Paused",
                    CurrentTargetFile = "Paused",
                    TotalFiles = job != null 
                        ? Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length 
                        : 0,
                    RemainingFiles = 0,
                    TotalSize = 0,
                    RemainingSize = 0,
                    ProgressPercentage = 0
                };

                NotifyObservers(state);
            }
        }

        /// <summary>
        /// Resumes the job with the specified <see cref="Guid"/>, if it was paused.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job to resume.</param>
        public void ResumeJob(Guid jobId)
        {
            if (_jobControllers.TryGetValue(jobId, out JobController controller) 
                && controller.State == JobState.Paused)
            {
                controller.State = JobState.Running;
                controller.PauseEvent.Set();
                Console.WriteLine($"Job {jobId} resumed.");

                var job = _backupJobs.FirstOrDefault(j => j.Id == jobId);
                var state = new BackupState
                {
                    JobId = jobId,
                    BackupName = job?.Name ?? "Unknown",
                    Status = "Running",
                    LastActionTime = DateTime.Now,
                    CurrentSourceFile = "Running",
                    CurrentTargetFile = "Running",
                    TotalFiles = job != null 
                        ? Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length 
                        : 0,
                    RemainingFiles = 0,
                    TotalSize = 0,
                    RemainingSize = 0,
                    ProgressPercentage = 0
                };

                NotifyObservers(state);
            }
        }

        /// <summary>
        /// Stops the job with the specified <see cref="Guid"/>, canceling any ongoing or pending work.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job to stop.</param>
        public void StopJob(Guid jobId)
        {
            if (_jobControllers.TryGetValue(jobId, out JobController controller))
            {
                controller.State = JobState.Stopped;
                controller.CancellationTokenSource.Cancel();
                controller.PauseEvent.Set();
                Console.WriteLine($"Job {jobId} stopped.");

                var job = _backupJobs.FirstOrDefault(j => j.Id == jobId);
                var state = new BackupState
                {
                    JobId = jobId,
                    BackupName = job?.Name ?? "Unknown",
                    Status = "Stopped",
                    LastActionTime = DateTime.Now,
                    CurrentSourceFile = "Stopped",
                    CurrentTargetFile = "Stopped",
                    TotalFiles = job != null 
                        ? Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories).Length 
                        : 0,
                    RemainingFiles = 0,
                    TotalSize = 0,
                    RemainingSize = 0,
                    ProgressPercentage = 0
                };

                NotifyObservers(state);
            }
        }

        #endregion

        #region Status Helpers

        /// <summary>
        /// Gets the current overall status of the backup manager.
        /// </summary>
        /// <returns>A string representing the manager's status.</returns>
        public string GetStatus()
        {
            return _status;
        }

        /// <summary>
        /// Checks if the configured business software is currently running.
        /// </summary>
        /// <returns><c>true</c> if the business software is detected; otherwise <c>false</c>.</returns>
        public bool IsBusinessSoftwareRunning()
        {
            return Process.GetProcessesByName(_businessSoftwareName).Any();
        }

        #endregion

        #region Private Utilities

        /// <summary>
        /// Saves changes to the list of backup jobs via the repository.
        /// </summary>
        private void SaveChanges()
        {
            _jobRepository.Save(_backupJobs);
        }

        /// <summary>
        /// Measures the current network usage in MB/s by reading bytes sent and received
        /// across all operational interfaces over a one-second interval.
        /// </summary>
        /// <returns>A double representing the total network usage in MB/s.</returns>
        private static double GetNetworkUsage()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.OperationalStatus == OperationalStatus.Up)
                .ToList();

            long totalBytesReceivedBefore = interfaces.Sum(i => i.GetIPv4Statistics().BytesReceived);
            long totalBytesSentBefore = interfaces.Sum(i => i.GetIPv4Statistics().BytesSent);

            Thread.Sleep(1000); // Wait one second to measure change

            long totalBytesReceivedAfter = interfaces.Sum(i => i.GetIPv4Statistics().BytesReceived);
            long totalBytesSentAfter = interfaces.Sum(i => i.GetIPv4Statistics().BytesSent);

            long totalReceivedPerSecond = totalBytesReceivedAfter - totalBytesReceivedBefore;
            long totalSentPerSecond = totalBytesSentAfter - totalBytesSentBefore;

            double totalUsageMBps = (totalReceivedPerSecond + totalSentPerSecond) / (1024.0 * 1024.0);
            return totalUsageMBps; // Return total usage in MB/s
        }

        #endregion
    }
}
