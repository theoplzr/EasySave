using EasySave.Core.Models;
using EasySaveLogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace EasySave.Core.Template
{
    /// <summary>
    /// An abstract class defining the template for backup algorithms.
    /// Implements the Template Method pattern to structure the backup process.
    /// </summary>
    public abstract class AbstractBackupAlgorithm
    {
        /// <summary>
        /// The logger instance used to record backup actions.
        /// </summary>
        protected readonly Logger _logger;

        /// <summary>
        /// A callback method to notify observers about changes in the backup state.
        /// </summary>
        private readonly Action<BackupState>? _notifyObserver;

        /// <summary>
        /// An action to persist changes in the backup state if needed.
        /// </summary>
        private readonly Action? _saveChanges;

        private readonly string _businessSoftwareName;
        private string _status;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBackupAlgorithm"/> class.
        /// </summary>
        /// <param name="logger">The logger instance to record backup actions.</param>
        /// <param name="notifyObserver">Action to notify observers about state changes.</param>
        /// <param name="saveChanges">Action to persist state changes.</param>
        /// <param name="businessSoftwareName">The name of the business software to detect.</param>
        protected AbstractBackupAlgorithm(
            Logger logger,
            Action<BackupState>? notifyObserver,
            Action? saveChanges,
            string businessSoftwareName
        )
        {
            _logger = logger;
            _notifyObserver = notifyObserver;
            _saveChanges = saveChanges;
            _businessSoftwareName = businessSoftwareName;
            _status = "waiting";
        }

        /// <summary>
        /// Executes the backup process following a defined template of steps.
        /// </summary>
        /// <param name="job">The backup job to execute.</param>
        public void Execute(BackupJob job)
        {
            // 1) Prepare the environment
            Prepare(job);

            // 2) Gather the files to back up
            var files = GatherFiles(job);

            // Tracking variables for file processing and progress
            int filesProcessed = 0;
            long bytesProcessed = 0;
            long totalSize = files.Sum(f => new FileInfo(f).Length);
            int totalFiles = files.Count();

            // 3) Process each file
            foreach (var file in files)
            {
                // If the business software is running, pause the backup
                while (IsBusinessSoftwareRunning())
                {
                    _status = "paused";
                    Thread.Sleep(2000); // Wait briefly before checking again
                }

                // Resume running state
                _status = "running";

                // If the file meets the criteria defined by the derived class, copy it
                if (ShouldCopyFile(file, job))
                {
                    CopyFile(job, file, ref filesProcessed, ref bytesProcessed, totalFiles, totalSize);
                }
            }

            // 4) Finalize the backup process
            FinalizeBackup(job);

            // Optionally persist changes
            _saveChanges?.Invoke();
        }

        /// <summary>
        /// Prepares the environment for the backup job.
        /// </summary>
        /// <param name="job">The current backup job.</param>
        protected virtual void Prepare(BackupJob job)
        {
            _status = "preparation";
            Console.WriteLine($"[Template] Starting backup '{job.Name}'...");

            // Verify that the source directory exists
            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.WriteLine($"The source directory does not exist: {job.SourceDirectory}");
            }

            // Verify that the target directory exists; if not, create it
            if (!Directory.Exists(job.TargetDirectory))
            {
                if (!string.IsNullOrEmpty(job.TargetDirectory))
                {
                    Directory.CreateDirectory(job.TargetDirectory);
                }
            }
        }

        /// <summary>
        /// Gathers all files to be backed up from the source directory.
        /// </summary>
        /// <param name="job">The backup job containing source details.</param>
        /// <returns>A collection of file paths to be backed up.</returns>
        protected virtual IEnumerable<string> GatherFiles(BackupJob job)
        {
            return Directory.GetFiles(job.SourceDirectory, "*.*", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Determines whether a file should be copied based on the job's configuration.
        /// Implemented by derived classes (e.g., full vs. differential).
        /// </summary>
        /// <param name="filePath">The file path to evaluate.</param>
        /// <param name="job">The backup job instance.</param>
        /// <returns><c>true</c> if the file should be copied; otherwise, <c>false</c>.</returns>
        protected abstract bool ShouldCopyFile(string filePath, BackupJob job);

        /// <summary>
        /// Copies a file from the source directory to the target directory.
        /// Logic to handle encryption or logging is also placed here.
        /// </summary>
        /// <param name="job">The backup job instance.</param>
        /// <param name="filePath">The file path of the file to copy.</param>
        /// <param name="filesProcessed">Reference to the count of already processed files.</param>
        /// <param name="bytesProcessed">Reference to the total processed file size in bytes.</param>
        /// <param name="totalFiles">Total number of files to process.</param>
        /// <param name="totalSize">Total size of all files to process.</param>
        protected abstract void CopyFile(
            BackupJob job,
            string filePath,
            ref int filesProcessed,
            ref long bytesProcessed,
            int totalFiles,
            long totalSize
        );

        /// <summary>
        /// Performs any final actions after the backup is complete.
        /// </summary>
        /// <param name="job">The completed backup job.</param>
        protected virtual void FinalizeBackup(BackupJob job)
        {
            _status = "finished";
            Console.WriteLine($"[Template] Backup '{job.Name}' completed.");
        }

        /// <summary>
        /// Notifies registered observers about the current backup state.
        /// </summary>
        /// <param name="state">An updated <see cref="BackupState"/> representing the current backup state.</param>
        protected void Notify(BackupState state)
        {
            _notifyObserver?.Invoke(state);
        }

        /// <summary>
        /// Logs an action to the underlying logging system.
        /// </summary>
        /// <param name="entry">The <see cref="LogEntry"/> to record.</param>
        protected void LogAction(LogEntry entry)
        {
            _logger.LogAction(entry);
        }

        /// <summary>
        /// Gets the current status of the backup (e.g., waiting, paused, running, finished).
        /// </summary>
        /// <returns>A string representing the current status.</returns>
        public string GetStatus()
        {
            return _status;
        }

        /// <summary>
        /// Checks if the specified business software is currently running.
        /// </summary>
        /// <returns><c>true</c> if the software is detected; otherwise, <c>false</c>.</returns>
        public bool IsBusinessSoftwareRunning()
        {
            return Process.GetProcessesByName(_businessSoftwareName).Any();
        }
    }
}
