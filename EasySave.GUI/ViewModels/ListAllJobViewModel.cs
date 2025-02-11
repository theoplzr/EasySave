using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Avalonia.Controls;
using EasySave.Core.Models;
using EasySave.Core.Facade;
using System.Reactive.Linq;
using System;

namespace EasySave.GUI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying and managing the list of all backup jobs.
    /// This class handles listing, selecting, and removing backup jobs.
    /// </summary>
    public class ListAllJobViewModel : ReactiveObject
    {
        /// <summary>
        /// Reference to the window associated with this ViewModel.
        /// Used to close the window when needed.
        /// </summary>
        private readonly Window? _window;
        /// <summary>
        /// Facade providing access to the backup jobs management logic.
        /// </summary>
        private readonly EasySaveFacade _facade;
        /// <summary>
        /// The currently selected backup job in the UI.
        /// </summary>
        private BackupJob? _selectedBackupJob;
        /// <summary>
        /// Collection of backup jobs to be displayed in the UI.
        /// </summary>
        public ObservableCollection<BackupJob> BackupJobs { get; }
        /// <summary>
        /// Command to close the window.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        /// <summary>
        /// Command to remove the selected backup job.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
        /// <summary>
        /// Gets or sets the currently selected backup job.
        /// When a job is selected in the UI, this property is updated.
        /// </summary>
        public BackupJob? SelectedBackupJob
        {
            get => _selectedBackupJob;
            set => this.RaiseAndSetIfChanged(ref _selectedBackupJob, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListAllJobViewModel"/> class.
        /// </summary>
        /// <param name="window">The window associated with this ViewModel.</param>
        /// <param name="facade">The facade providing access to backup job operations.</param>
        public ListAllJobViewModel(Window window, EasySaveFacade facade)
        {
            _window = window;
            _facade = facade;
            BackupJobs = new ObservableCollection<BackupJob>(_facade.ListBackupJobs());

            CancelCommand = ReactiveCommand.Create(Cancel);
            RemoveCommand = ReactiveCommand.Create(Remove, this.WhenAnyValue(x => x.SelectedBackupJob).Select(job => job != null));
        }

        /// <summary>
        /// Closes the window when the Cancel button is clicked.
        /// </summary>
        private void Cancel()
        {
            _window?.Close();
        }

        /// <summary>
        /// Removes the selected backup job from the list.
        /// If a job is selected, it finds the index and removes it from the facade and UI.
        /// </summary>
        private void Remove()
        {
            if (SelectedBackupJob != null)
            {
                if (SelectedBackupJob != null)
                {
                    Guid jobId = SelectedBackupJob.Id;
                    int index = _facade.GetJobIndexById(jobId);

                    if (index != -1)
                    {
                        _facade.RemoveJob(index); // Suppression du job
                        BackupJobs.RemoveAt(index); // Mise à jour de la liste affichée
                    }
                    else
                    {
                        Console.WriteLine("Job non trouvé.");
                    }
                }
            }
        }
    }
}
