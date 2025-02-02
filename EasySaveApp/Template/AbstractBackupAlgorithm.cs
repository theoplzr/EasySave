using EasySaveApp.Models;
using EasySaveLogs;

namespace EasySaveApp.Template
{
    public abstract class AbstractBackupAlgorithm
    {
        protected readonly Logger _logger;
        private readonly Action<BackupState>? _notifyObserver;
        private readonly Action? _saveChanges;

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

        public void Execute(BackupJob job)
        {
            Prepare(job);
            var files = GatherFiles(job);

            // Gestions totaux pour logs/état
            int filesProcessed = 0;
            long bytesProcessed = 0;
            long totalSize = files.Sum(f => new FileInfo(f).Length);
            int totalFiles = files.Count();

            foreach (var file in files)
            {
                if (ShouldCopyFile(file, job))
                {
                    CopyFile(job, file, ref filesProcessed, ref bytesProcessed, totalFiles, totalSize);
                }
            }
            FinalizeBackup(job);

            // Si on souhaite sauvegarder la liste des jobs après
            _saveChanges?.Invoke();
        }

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

        protected virtual IEnumerable<string> GatherFiles(BackupJob job)
        {
            return Directory.GetFiles(job.SourceDirectory, "*.*", SearchOption.AllDirectories);
        }

        protected abstract bool ShouldCopyFile(string filePath, BackupJob job);

        protected abstract void CopyFile(
            BackupJob job,
            string filePath,
            ref int filesProcessed,
            ref long bytesProcessed,
            int totalFiles,
            long totalSize
        );

        protected virtual void FinalizeBackup(BackupJob job)
        {
            Console.WriteLine($"[Template] Backup '{job.Name}' completed.");
        }

        protected void Notify(BackupState state)
        {
            _notifyObserver?.Invoke(state);
        }

        protected void LogAction(LogEntry entry)
        {
            _logger.LogAction(entry);
        }
    }
}
