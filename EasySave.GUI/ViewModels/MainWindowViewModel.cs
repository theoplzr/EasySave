using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using EasySave.Core.Models;
using EasySave.Core.Facade;
using EasySave.Core.Repositories;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EasySave.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly EasySaveFacade _facade;

        public ObservableCollection<BackupJob> BackupJobs { get; }
        private BackupJob _selectedJob;
        public BackupJob SelectedJob
        {
            get => _selectedJob;
            set => this.RaiseAndSetIfChanged(ref _selectedJob, value);
        }

        private string _realTimeStatus;
        public string RealTimeStatus
        {
            get => _realTimeStatus;
            set => this.RaiseAndSetIfChanged(ref _realTimeStatus, value);
        }

        // Commandes
        public ReactiveCommand<Unit, Unit> AddJobCommand { get; }
        public ReactiveCommand<Unit, Unit> ModifyJobCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteJobCommand { get; }
        public ReactiveCommand<Unit, Unit> ExecuteJobCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenConfigurationCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }

        public MainWindowViewModel()
        {
            // Construction de l'objet IConfiguration à partir du fichier appsettings.json
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = configBuilder.Build();

            // Instanciation de la façade en passant le repository, le logDirectory, l'observer (ici null) et la configuration
            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("backup_jobs.json"),
                "Logs",
                null,
                configuration
            );

            // Charger la liste des jobs depuis la façade
            BackupJobs = new ObservableCollection<BackupJob>(_facade.ListBackupJobs());
            RealTimeStatus = "Idle";

            // Initialisation des commandes
            AddJobCommand = ReactiveCommand.Create(AddJob);
            ModifyJobCommand = ReactiveCommand.Create(ModifyJob);
            DeleteJobCommand = ReactiveCommand.Create(DeleteJob);
            ExecuteJobCommand = ReactiveCommand.Create(ExecuteJob);
            OpenConfigurationCommand = ReactiveCommand.Create(OpenConfiguration);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
        }

        private void AddJob()
        {
            // Pour simplifier, on ajoute un job factice.
            var newJob = new BackupJob("New Job", "/source", "/target", BackupType.Complete);
            _facade.AddJob(newJob);
            BackupJobs.Add(newJob);
            RealTimeStatus = "Job added.";
        }

        private void ModifyJob()
        {
            if (SelectedJob == null)
            {
                RealTimeStatus = "No job selected to modify.";
                return;
            }
            // Exemple : modifier le nom (dans une application réelle, vous ouvrirez une fenêtre de modification)
            string newName = SelectedJob.Name + " (Modified)";
            int index = BackupJobs.IndexOf(SelectedJob);
            _facade.UpdateJob(index, newName, null, null, null);
            SelectedJob.Name = newName;
            RealTimeStatus = "Job modified.";
        }

        private void DeleteJob()
        {
            if (SelectedJob == null)
            {
                RealTimeStatus = "No job selected to delete.";
                return;
            }
            int index = BackupJobs.IndexOf(SelectedJob);
            _facade.RemoveJob(index);
            BackupJobs.RemoveAt(index);
            RealTimeStatus = "Job deleted.";
        }

        private void ExecuteJob()
        {
            if (SelectedJob == null)
            {
                RealTimeStatus = "No job selected to execute.";
                return;
            }
            int index = BackupJobs.IndexOf(SelectedJob);
            _facade.ExecuteJobByIndex(index);
            RealTimeStatus = "Job executed.";
        }

        private void OpenConfiguration()
        {
            // Ouvre la fenêtre de configuration.
            var configWindow = new Views.ConfigurationWindow();
            configWindow.Show();
        }
    }
}
