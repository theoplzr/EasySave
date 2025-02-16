using System;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using ReactiveUI;
using EasySave.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using System.IO;
using EasySave.GUI.Helpers;

namespace EasySave.GUI.ViewModels
{
    public class JobFormViewModel : ReactiveObject
    {
        private string _name;
        private string _sourceDirectory;
        private string _targetDirectory;
        private BackupType _backupType;
        private string? _nameError = string.Empty;
        private string? _sourceDirectoryError = string.Empty;
        private string? _targetDirectoryError = string.Empty;
        private bool _hasAttemptedSave = false;
        public ObservableCollection<BackupType> BackupTypes { get; }

        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        public string Name
        {
            get => _name;
            set
            {
                this.RaiseAndSetIfChanged(ref _name, value);
                if (_hasAttemptedSave)
                    ValidateForm();
            }
        }

        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                this.RaiseAndSetIfChanged(ref _sourceDirectory, value);
                if (_hasAttemptedSave)
                    ValidateForm();
            }
        }

        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                this.RaiseAndSetIfChanged(ref _targetDirectory, value);
                if (_hasAttemptedSave)
                    ValidateForm();
            }
        }

        public BackupType BackupType
        {
            get => _backupType;
            set => this.RaiseAndSetIfChanged(ref _backupType, value);
        }

        public string? NameError
        {
            get => _nameError;
            private set => this.RaiseAndSetIfChanged(ref _nameError, value);
        }

        public string? SourceDirectoryError
        {
            get => _sourceDirectoryError;
            private set => this.RaiseAndSetIfChanged(ref _sourceDirectoryError, value);
        }

        public string? TargetDirectoryError
        {
            get => _targetDirectoryError;
            private set => this.RaiseAndSetIfChanged(ref _targetDirectoryError, value);
        }

        public bool CanSave => string.IsNullOrEmpty(NameError) &&
                               string.IsNullOrEmpty(SourceDirectoryError) &&
                               string.IsNullOrEmpty(TargetDirectoryError);

        // On modifie le type du SaveCommand en BackupJob? pour refléter un retour possible null.
        public ReactiveCommand<Unit, BackupJob?> SaveCommand { get; } 
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        // Commandes pour ouvrir l'explorateur de dossiers
        public ReactiveCommand<Unit, Unit> BrowseSourceCommand { get; }
        public ReactiveCommand<Unit, Unit> BrowseTargetCommand { get; }

        private readonly Window? _window; 

        public JobFormViewModel(Window window, BackupJob? existingJob = null)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

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

            // Initialisation des commandes pour parcourir les répertoires
            BrowseSourceCommand = ReactiveCommand.CreateFromTask(BrowseSourceDirectoryAsync);
            BrowseTargetCommand = ReactiveCommand.CreateFromTask(BrowseTargetDirectoryAsync);
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

            // Dans ce constructeur sans fenêtre, les commandes de navigation ne feront rien
            BrowseSourceCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);
            BrowseTargetCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);
        }

        private BackupJob? SaveJob()
        {
            _hasAttemptedSave = true; 
            ValidateForm();

            if (CanSave)
            {
                var job = new BackupJob(Name, SourceDirectory, TargetDirectory, BackupType);
                _window?.Close(job);
                return job;
            }
            else
            {
                return null;
            }
        }

        private void ValidateForm()
        {
            // Vérification du champ Name
            NameError = string.IsNullOrWhiteSpace(Name) ? "Please enter a name for the job." : null;

            // Vérification du champ SourceDirectory
            if (string.IsNullOrWhiteSpace(SourceDirectory) ||
                !Directory.Exists(SourceDirectory) ||
                !Path.IsPathFullyQualified(SourceDirectory))
            {
                SourceDirectoryError = "Please enter a valid source directory.";
            }
            else
            {
                SourceDirectoryError = null;
            }

            // Vérification du champ TargetDirectory
            if (string.IsNullOrWhiteSpace(TargetDirectory) ||
                !Directory.Exists(TargetDirectory) ||
                !Path.IsPathFullyQualified(TargetDirectory))
            {
                TargetDirectoryError = "Please enter a valid target directory.";
            }
            else
            {
                TargetDirectoryError = null;
            }

            this.RaisePropertyChanged(nameof(CanSave)); // Met à jour l'état du bouton Save
        }

        private void Cancel()
        {
            _window?.Close(); 
        }

        /// <summary>
        /// Ouvre l'explorateur de dossiers pour sélectionner le répertoire source.
        /// </summary>
        private async Task BrowseSourceDirectoryAsync()
        {
            if (_window == null)
                return;

#pragma warning disable CS0618 // Suppression du warning d'obsolescence de OpenFolderDialog
            var dialog = new OpenFolderDialog
            {
                Title = "Select Source Directory"
            };

            var result = await dialog.ShowAsync(_window);
#pragma warning restore CS0618

            if (!string.IsNullOrWhiteSpace(result))
            {
                SourceDirectory = result;
            }
        }

        /// <summary>
        /// Ouvre l'explorateur de dossiers pour sélectionner le répertoire cible.
        /// </summary>
        private async Task BrowseTargetDirectoryAsync()
        {
            if (_window == null)
                return;

#pragma warning disable CS0618 // Suppression du warning d'obsolescence de OpenFolderDialog
            var dialog = new OpenFolderDialog
            {
                Title = "Select Target Directory"
            };

            var result = await dialog.ShowAsync(_window);
#pragma warning restore CS0618

            if (!string.IsNullOrWhiteSpace(result))
            {
                TargetDirectory = result;
            }
        }
    }
}
