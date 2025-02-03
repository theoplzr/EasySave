using EasySaveApp.Models;
using EasySaveLogs;

namespace EasySaveApp.Template
{
    /// <summary>
    /// Abstract class defining the template for backup algorithms.
    /// Implements the Template Method pattern to structure the backup process.
    /// </summary>
    public abstract class AbstractBackupAlgorithm
    {
        /// <summary>
        /// Logger instance to record backup activities.
        /// </summary>
        protected readonly Logger _logger;

        /// <summary>
        /// Action to notify observers about backup state changes.
        /// </summary>
        private readonly Action<BackupState>? _notifyObserver;

        /// <summary>
        /// Action to save changes to the backup state.
        /// </summary>
        private readonly Action? _saveChanges;

        /// <summary>
        /// Constructor for the abstract backup algorithm.
        /// </summary>
        /// <param name="logger">Logger instance for recording actions.</param>
        /// <param name="notifyObserver">Action to notify observers about state changes.</param>
        /// <param name="saveChanges">Action to persist backup state changes.</param>
        protected AbstractBackupAlgorithm(
            Logger logger,
            Action<BackupState>? notifyObserver,
            Action? saveChanges
        )
        {
            _logger = logger;
            _notifyObserver = notifyObserver;
            _saveChanges = saveChanges;
        }

        /// <summary>
        /// Executes the backup process following the defined logic.
        /// </summary>
        /// <param name="job">The backup job to execute.</param>
        public void Execute(BackupJob job)
        {
            Prepare(job); // Prepare the backup job
            var files = GatherFiles(job); // Retrieve files to back up

            // Tracking variables for progress logging
            int filesProcessed = 0;
            long bytesProcessed = 0;
            long totalSize = files.Sum(f => new FileInfo(f).Length);
            int totalFiles = files.Count();

            // Process each file
            foreach (var file in files)
            {
                if (ShouldCopyFile(file, job))
                {
                    CopyFile(job, file, ref filesProcessed, ref bytesProcessed, totalFiles, totalSize);
                }
            }

            FinalizeBackup(job); // Finalize the backup process

            // Save changes if needed
            _saveChanges?.Invoke();
        }

        /// <summary>
        /// Prepares the environment for the backup job.
        /// </summary>
        /// <param name="job">The current backup job.</param>
        protected virtual void Prepare(BackupJob job)
        {
            Console.WriteLine($"[Template] Starting backup {job.Name} ...");

            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.WriteLine($"Source directory does not exist: {job.SourceDirectory}");
            }

            if (!Directory.Exists(job.TargetDirectory))
            {
                if (!string.IsNullOrEmpty(job.TargetDirectory))
                {
                    Directory.CreateDirectory(job.TargetDirectory);
                }
            }
        }

        /// <summary>
        /// Retrieves the list of files to be backed up from the source directory.
        /// </summary>
        /// <param name="job">The backup job.</param>
        /// <returns>A collection of file paths to be backed up.</returns>
        protected virtual IEnumerable<string> GatherFiles(BackupJob job)
        {
            return Directory.GetFiles(job.SourceDirectory, "*.*", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Determines whether a file should be copied based on the backup job configuration.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="job">The backup job.</param>
        /// <returns>Returns true if the file should be copied, otherwise false.</returns>
        protected abstract bool ShouldCopyFile(string filePath, BackupJob job);

        /// <summary>
        /// Copies a file from the source directory to the target directory.
        /// </summary>
        /// <param name="job">The backup job.</param>
        /// <param name="filePath">The file path to be copied.</param>
        /// <param name="filesProcessed">Reference to the count of processed files.</param>
        /// <param name="bytesProcessed">Reference to the total size of processed files.</param>
        /// <param name="totalFiles">Total number of files to be processed.</param>
        /// <param name="totalSize">Total size of files to be backed up.</param>
        protected abstract void CopyFile(
            BackupJob job,
            string filePath,
            ref int filesProcessed,
            ref long bytesProcessed,
            int totalFiles,
            long totalSize
        );

        /// <summary>
        /// Performs final actions after the backup process is complete.
        /// </summary>
        /// <param name="job">The completed backup job.</param>
        protected virtual void FinalizeBackup(BackupJob job)
        {
            Console.WriteLine($"[Template] Backup '{job.Name}' completed.");
        }

        /// <summary>
        /// Notifies observers of the current backup state.
        /// </summary>
        /// <param name="state">The current backup state.</param>
        protected void Notify(BackupState state)
        {
            _notifyObserver?.Invoke(state);
        }

        /// <summary>
        /// Logs an action in the log system.
        /// </summary>
        /// <param name="entry">The log entry to be recorded.</param>
        protected void LogAction(LogEntry entry)
        {
            _logger.LogAction(entry);
        }
    }
}
