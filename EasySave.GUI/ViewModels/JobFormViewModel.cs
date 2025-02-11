using System;
using System.Reactive;
using ReactiveUI;
using EasySave.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;

namespace EasySave.GUI.ViewModels
{
    public class JobFormViewModel : ReactiveObject
    {
        private string _name;
        private string _sourceDirectory;
        private string _targetDirectory;
        private BackupType _backupType;
        public ObservableCollection<BackupType> BackupTypes { get; }

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string SourceDirectory
        {
            get => _sourceDirectory;
            set => this.RaiseAndSetIfChanged(ref _sourceDirectory, value);
        }

        public string TargetDirectory
        {
            get => _targetDirectory;
            set => this.RaiseAndSetIfChanged(ref _targetDirectory, value);
        }

        public BackupType BackupType
        {
            get => _backupType;
            set => this.RaiseAndSetIfChanged(ref _backupType, value);
        }

        public ReactiveCommand<Unit, BackupJob> SaveCommand { get; } 
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private readonly Window? _window; 

        public JobFormViewModel(Window window, BackupJob? existingJob = null)
        {
            _window = window; 

            BackupTypes = new ObservableCollection<BackupType>(Enum.GetValues(typeof(BackupType)).Cast<BackupType>());

            if (existingJob != null)
            {
                _name = existingJob.Name;
                _sourceDirectory = existingJob.SourceDirectory;
                _targetDirectory = existingJob.TargetDirectory;
                _backupType = existingJob.BackupType;
            }
            else
            {
                _name = "";
                _sourceDirectory = "";
                _targetDirectory = "";
                _backupType = BackupType.Complete;
            }

            SaveCommand = ReactiveCommand.Create(SaveJob);
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        public JobFormViewModel() 
        {
            BackupTypes = new ObservableCollection<BackupType>(Enum.GetValues(typeof(BackupType)).Cast<BackupType>());
            _name = "";
            _sourceDirectory = "";
            _targetDirectory = "";
            _backupType = BackupType.Complete;

            SaveCommand = ReactiveCommand.Create(SaveJob); 
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        private BackupJob SaveJob()
        {
            var job = new BackupJob(Name, SourceDirectory, TargetDirectory, BackupType);
            _window?.Close(job); 
            return job;
        }

        private void Cancel()
        {
            _window?.Close(); 
        }
    }
}
