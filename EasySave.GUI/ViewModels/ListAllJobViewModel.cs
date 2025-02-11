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
    public class ListAllJobViewModel : ReactiveObject
    {
        private readonly Window? _window;
        private readonly EasySaveFacade _facade;
        private BackupJob? _selectedBackupJob;
        public ObservableCollection<BackupJob> BackupJobs { get; }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        public BackupJob? SelectedBackupJob
        {
            get => _selectedBackupJob;
            set => this.RaiseAndSetIfChanged(ref _selectedBackupJob, value);
        }

        public ListAllJobViewModel(Window window, EasySaveFacade facade)
        {
            _window = window;
            _facade = facade;
            BackupJobs = new ObservableCollection<BackupJob>(_facade.ListBackupJobs());

            CancelCommand = ReactiveCommand.Create(Cancel);
            RemoveCommand = ReactiveCommand.Create(Remove, this.WhenAnyValue(x => x.SelectedBackupJob).Select(job => job != null));
        }

        private void Cancel()
        {
            _window?.Close();
        }

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
