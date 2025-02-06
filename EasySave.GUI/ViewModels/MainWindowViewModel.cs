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
        private BackupJob _selectedJob = new BackupJob("Default Job", "/default/source", "/default/target", BackupType.Complete);

        public BackupJob SelectedJob
        {
            get => _selectedJob;
            set => this.RaiseAndSetIfChanged(ref _selectedJob, value);
        }

        private string _realTimeStatus = "";
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
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = configBuilder.Build();

            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("backup_jobs.json"),
                "Logs",
                null,
                configuration
            );

            BackupJobs = new ObservableCollection<BackupJob>(_facade.ListBackupJobs());
            RealTimeStatus = "Idle";

            AddJobCommand = ReactiveCommand.Create(AddJob);
            ModifyJobCommand = ReactiveCommand.Create(ModifyJob);
            DeleteJobCommand = ReactiveCommand.Create(DeleteJob);
            ExecuteJobCommand = ReactiveCommand.Create(ExecuteJob);
            OpenConfigurationCommand = ReactiveCommand.Create(OpenConfiguration);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
        }

        private void AddJob()
        {
            var newJob = new BackupJob("New Job", "/source", "/target", BackupType.Complete);
            _facade.AddJob(newJob);
            BackupJobs.Add(newJob);
            RealTimeStatus = "Job added.";
        }

        private void ModifyJob()
        {
            if (SelectedJob == null) return;
            SelectedJob.Name += " (Modified)";
            RealTimeStatus = "Job modified.";
        }

        private void DeleteJob()
        {
            if (SelectedJob == null) return;
            BackupJobs.Remove(SelectedJob);
            RealTimeStatus = "Job deleted.";
        }

        private void ExecuteJob()
        {
            if (SelectedJob == null) return;
            RealTimeStatus = "Job executed.";
        }

        private void OpenConfiguration()
        {
            var configWindow = new Views.ConfigurationWindow();
            configWindow.Show();
        }
    }
}
