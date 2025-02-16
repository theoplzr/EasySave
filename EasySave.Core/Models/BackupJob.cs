using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySave.Core.Models.BackupStrategies;
using Newtonsoft.Json;

namespace EasySave.Core.Models
{
    public class BackupJob : INotifyPropertyChanged
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public BackupType BackupType { get; set; }

        [JsonIgnore]
        public IBackupStrategy _backupStrategy;

        public int Ordinal { get; set; }

        private bool _isSelected;
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BackupJob(string name, string sourceDirectory, string targetDirectory, BackupType backupType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "The backup job name cannot be null.");
            SourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory), "The source directory cannot be null.");
            TargetDirectory = targetDirectory ?? throw new ArgumentNullException(nameof(targetDirectory), "The target directory cannot be null.");
            BackupType = backupType;
            _backupStrategy = BackupStrategyFactory.GetStrategy(backupType);
        }

        public override string ToString()
        {
            return $"Backup Job: {Name}, Source: {SourceDirectory}, Target: {TargetDirectory}, Type: {BackupType}";
        }
    }
}