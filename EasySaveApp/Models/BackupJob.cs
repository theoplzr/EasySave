using EasySaveApp.Models.BackupStrategies;
using Newtonsoft.Json;

namespace EasySaveApp.Models
{
    /// <summary>
    /// Represents a backup job.
    /// A backup job is defined by a name, a source directory, a target directory, and the backup type (full or differential).
    /// </summary>
    public class BackupJob
    {
        /// <summary>
        /// Unique identifier for the backup job.
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Name of the backup job.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Source directory containing the files to be backed up.
        /// </summary>
        public string SourceDirectory { get; set; }

        /// <summary>
        /// Target directory where the backed-up files will be stored.
        /// </summary>
        public string TargetDirectory { get; set; }

        /// <summary>
        /// Type of backup: Complete or Differential.
        /// </summary>
        public BackupType BackupType { get; set; }

        /// <summary>
        /// Backup strategy instance, used to determine the specific backup logic.
        /// This property is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore] // Excluded from serialization
        public IBackupStrategy _backupStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupJob"/> class.
        /// </summary>
        /// <param name="name">The name of the backup job.</param>
        /// <param name="sourceDirectory">The source directory containing the files to back up.</param>
        /// <param name="targetDirectory">The target directory where the files will be saved.</param>
        /// <param name="backupType">The type of backup (Complete or Differential).</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public BackupJob(string name, string sourceDirectory, string targetDirectory, BackupType backupType)
        {
            // Validate parameters to prevent null values
            Name = name ?? throw new ArgumentNullException(nameof(name), "The backup job name cannot be null.");
            SourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory), "The source directory cannot be null.");
            TargetDirectory = targetDirectory ?? throw new ArgumentNullException(nameof(targetDirectory), "The target directory cannot be null.");
            BackupType = backupType;

            // Assign the appropriate backup strategy
            _backupStrategy = BackupStrategyFactory.GetStrategy(backupType);
        }

        /// <summary>
        /// Returns a descriptive string representation of the backup job.
        /// This method is useful for logging and displaying backup job details in the user interface.
        /// </summary>
        /// <returns>A string containing the backup job details (name, source, target, and type).</returns>
        public override string ToString()
        {
            return $"Backup Job: {Name}, Source: {SourceDirectory}, Target: {TargetDirectory}, Type: {BackupType}";
        }
    }
}
