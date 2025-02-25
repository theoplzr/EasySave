using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySave.Core.Models.BackupStrategies;
using Newtonsoft.Json;

namespace EasySave.Core.Models
{
    /// <summary>
    /// Represents a backup job that defines a source, target, and backup strategy.
    /// </summary>
    public class BackupJob : INotifyPropertyChanged
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
        /// The directory where files will be backed up from.
        /// </summary>
        public string SourceDirectory { get; set; }

        /// <summary>
        /// The target directory where files will be saved.
        /// </summary>
        public string TargetDirectory { get; set; }

        /// <summary>
        /// The type of backup (Complete or Differential).
        /// </summary>
        public BackupType BackupType { get; set; }

        /// <summary>
        /// The backup strategy used for this job (determined by the backup type).
        /// </summary>
        [JsonIgnore]
        public IBackupStrategy _backupStrategy;

        /// <summary>
        /// Order index of the backup job (useful for UI sorting).
        /// </summary>
        public int Ordinal { get; set; }

        private bool _isSelected;

        /// <summary>
        /// Indicates whether the backup job is selected (useful for UI interactions).
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _status = "Idle"; // État par défaut
        /// <summary>
        /// Status of the backup job (Running, Paused, Stopped, Finished).
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Event triggered when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifies UI components when a property changes.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupJob"/> class.
        /// </summary>
        /// <param name="name">The name of the backup job.</param>
        /// <param name="sourceDirectory">The source directory.</param>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="backupType">The type of backup (Complete or Differential).</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public BackupJob(string name, string sourceDirectory, string targetDirectory, BackupType backupType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "The backup job name cannot be null.");
            SourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory), "The source directory cannot be null.");
            TargetDirectory = targetDirectory ?? throw new ArgumentNullException(nameof(targetDirectory), "The target directory cannot be null.");
            BackupType = backupType;
            _backupStrategy = BackupStrategyFactory.GetStrategy(backupType);
        }

        /// <summary>
        /// Returns a string representation of the backup job.
        /// </summary>
        /// <returns>A formatted string with job details.</returns>
        public override string ToString()
        {
            return $"Backup Job: {Name}, Source: {SourceDirectory}, Target: {TargetDirectory}, Type: {BackupType}";
        }
    }
}