using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Avalonia.Controls;
using EasySave.Core.Models;
using EasySave.Core.Facade;

namespace EasySave.GUI.ViewModels
{
    public class ListAllJobViewModel : ReactiveObject
    {
        private readonly Window? _window;
        private readonly EasySaveFacade _facade; // Accès aux jobs
        public ObservableCollection<BackupJob> BackupJobs { get; }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public ListAllJobViewModel(Window window, EasySaveFacade facade)
        {
            _window = window;
            _facade = facade;
            BackupJobs = new ObservableCollection<BackupJob>(_facade.ListBackupJobs());

            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        private void Cancel()
        {
            _window?.Close();
        }
    }
}
