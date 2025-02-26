using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia.Controls;
using EasySave.Core.Models;
using EasySave.GUI.Helpers;
using ReactiveUI;

namespace EasySave.GUI.ViewModels
{
    /// <summary>
    /// ViewModel for the job creation or modification form.
    /// Handles user input, validation, and creation/modification of a <see cref="BackupJob"/>.
    /// </summary>
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

        private readonly Window? _window;

        /// <summary>
        /// A collection of available backup types (e.g., Complete, Differential).
        /// </summary>
        public ObservableCollection<BackupType> BackupTypes { get; }

        /// <summary>
        /// Gets the current <see cref="LanguageHelper"/> instance for localization.
        /// </summary>
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        /// <summary>
        /// Gets or sets the job name.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                this.RaiseAndSetIfChanged(ref _name, value);
                if (_hasAttemptedSave)
                {
                    ValidateForm();
                }
            }
        }

        /// <summary>
        /// Gets or sets the source directory path for backup files.
        /// </summary>
        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                this.RaiseAndSetIfChanged(ref _sourceDirectory, value);
                if (_hasAttemptedSave)
                {
                    ValidateForm();
                }
            }
        }

        /// <summary>
        /// Gets or sets the target directory path where backup files will be placed.
        /// </summary>
        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                this.RaiseAndSetIfChanged(ref _targetDirectory, value);
                if (_hasAttemptedSave)
                {
                    ValidateForm();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="BackupType"/>.
        /// </summary>
        public BackupType BackupType
        {
            get => _backupType;
            set => this.RaiseAndSetIfChanged(ref _backupType, value);
        }

        /// <summary>
        /// Gets or sets an error message for the job name field if validation fails.
        /// </summary>
        public string? NameError
        {
            get => _nameError;
            private set => this.RaiseAndSetIfChanged(ref _nameError, value);
        }

        /// <summary>
        /// Gets or sets an error message for the source directory field if validation fails.
        /// </summary>
        public string? SourceDirectoryError
        {
            get => _sourceDirectoryError;
            private set => this.RaiseAndSetIfChanged(ref _sourceDirectoryError, value);
        }

        /// <summary>
        /// Gets or sets an error message for the target directory field if validation fails.
        /// </summary>
        public string? TargetDirectoryError
        {
            get => _targetDirectoryError;
            private set => this.RaiseAndSetIfChanged(ref _targetDirectoryError, value);
        }

        /// <summary>
        /// Indicates whether the form is valid and can be saved.
        /// </summary>
        public bool CanSave =>
            string.IsNullOrEmpty(NameError) &&
            string.IsNullOrEmpty(SourceDirectoryError) &&
            string.IsNullOrEmpty(TargetDirectoryError);

        // Reactive Commands
        public ReactiveCommand<Unit, BackupJob?> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> BrowseSourceCommand { get; }
        public ReactiveCommand<Unit, Unit> BrowseTargetCommand { get; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JobFormViewModel"/>,
        /// primarily for creating or editing a job in a given window context.
        /// </summary>
        /// <param name="window">The parent window for this form.</param>
        /// <param name="existingJob">An optional existing job to edit.</param>
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
                _name = string.Empty;
                _sourceDirectory = string.Empty;
                _targetDirectory = string.Empty;
                _backupType = BackupType.Complete;
            }

            // Command initializations
            SaveCommand = ReactiveCommand.Create(SaveJob);
            CancelCommand = ReactiveCommand.Create(Cancel);
            BrowseSourceCommand = ReactiveCommand.CreateFromTask(BrowseSourceDirectoryAsync);
            BrowseTargetCommand = ReactiveCommand.CreateFromTask(BrowseTargetDirectoryAsync);
        }

        /// <summary>
        /// Default constructor for scenarios without a parent window context.
        /// Useful for certain design-time or testing scenarios.
        /// </summary>
        public JobFormViewModel()
        {
            BackupTypes = new ObservableCollection<BackupType>(Enum.GetValues(typeof(BackupType)).Cast<BackupType>());
            _name = string.Empty;
            _sourceDirectory = string.Empty;
            _targetDirectory = string.Empty;
            _backupType = BackupType.Complete;

            SaveCommand = ReactiveCommand.Create(SaveJob);
            CancelCommand = ReactiveCommand.Create(Cancel);
            BrowseSourceCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);
            BrowseTargetCommand = ReactiveCommand.CreateFromTask(() => Task.CompletedTask);
        }

        #endregion

        /// <summary>
        /// Attempts to validate and save the current job if the form is valid.
        /// Closes the window upon successful creation.
        /// </summary>
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
            return null;
        }

        /// <summary>
        /// Validates the form fields for name, source directory, and target directory.
        /// </summary>
        private void ValidateForm()
        {
            // Validate Name
            NameError = string.IsNullOrWhiteSpace(Name)
                ? "Please enter a name for the job."
                : null;

            // Validate SourceDirectory
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

            // Validate TargetDirectory
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

            // Raise property changed for CanSave to update UI binding
            this.RaisePropertyChanged(nameof(CanSave));
        }

        /// <summary>
        /// Cancels the job creation or modification process, closing the form without saving.
        /// </summary>
        private void Cancel()
        {
            _window?.Close();
        }

        /// <summary>
        /// Opens a folder browser dialog to select the source directory.
        /// </summary>
        private async Task BrowseSourceDirectoryAsync()
        {
            if (_window == null)
                return;

            var dialog = new OpenFolderDialog
            {
                Title = "Select Source Directory"
            };
            var result = await dialog.ShowAsync(_window);

            if (!string.IsNullOrWhiteSpace(result))
            {
                SourceDirectory = result;
            }
        }

        /// <summary>
        /// Opens a folder browser dialog to select the target directory.
        /// </summary>
        private async Task BrowseTargetDirectoryAsync()
        {
            if (_window == null)
                return;

            var dialog = new OpenFolderDialog
            {
                Title = "Select Target Directory"
            };
            var result = await dialog.ShowAsync(_window);

            if (!string.IsNullOrWhiteSpace(result))
            {
                TargetDirectory = result;
            }
        }
    }
}
