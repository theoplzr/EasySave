using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using EasySave.Core.Models;
using EasySave.Core.Facade;
using EasySave.Core.Repositories;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reactive.Concurrency;
using EasySave.GUI.Utils;

namespace EasySave.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly EasySaveFacade _facade;

        public ObservableCollection<BackupJob> BackupJobs { get; }

        private BackupJob? _selectedJob;
        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedJob, value);
                if (value != null)
                {
                    RealTimeStatus = $"Selected job: {value.Name}";
                    Console.WriteLine($"Selected job: {value.Name}");
                }
                else
                {
                    RealTimeStatus = "No job selected";
                    Console.WriteLine("No job selected.");
                }
            }
        }

        private string _realTimeStatus = "Idle";
        public string RealTimeStatus
        {
            get => _realTimeStatus;
            set
            {
                this.RaiseAndSetIfChanged(ref _realTimeStatus, value);
                Console.WriteLine($"RealTimeStatus updated: {_realTimeStatus}");
            }
        }

        // Commandes
        public ReactiveCommand<Unit, Unit> AddJobCommand { get; }
        public ReactiveCommand<Unit, Unit> ModifyJobCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteJobCommand { get; }
        public ReactiveCommand<Unit, Unit> ExecuteJobCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenConfigurationCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }

        // Constructeur statique pour configurer le scheduler global
        static MainWindowViewModel()
        {
            RxApp.MainThreadScheduler = AvaloniaDispatcherScheduler.Instance;
        }

        public MainWindowViewModel()
        {
            Console.WriteLine("Initializing MainWindowViewModel...");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = configBuilder.Build();

            Console.WriteLine("Configuration loaded.");

            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("backup_jobs.json"),
                "Logs",
                null,
                configuration
            );

            Console.WriteLine("Facade initialized.");

            BackupJobs = new ObservableCollection<BackupJob>(_facade.ListBackupJobs());
            Console.WriteLine($"Loaded {BackupJobs.Count} backup jobs into the DataGrid.");
            RealTimeStatus = "Idle";

            // Création des commandes en précisant l'outputScheduler pour forcer l'exécution sur le thread UI
            AddJobCommand = ReactiveCommand.CreateFromTask(AddJobAsync, outputScheduler: RxApp.MainThreadScheduler);
            ModifyJobCommand = ReactiveCommand.CreateFromTask(ModifyJobAsync, outputScheduler: RxApp.MainThreadScheduler);
            DeleteJobCommand = ReactiveCommand.CreateFromTask(DeleteJobAsync, outputScheduler: RxApp.MainThreadScheduler);
            ExecuteJobCommand = ReactiveCommand.CreateFromTask(ExecuteJobAsync, outputScheduler: RxApp.MainThreadScheduler);
            OpenConfigurationCommand = ReactiveCommand.Create(OpenConfiguration, outputScheduler: RxApp.MainThreadScheduler);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0), outputScheduler: RxApp.MainThreadScheduler);
        }

        private async Task AddJobAsync()
        {
            Console.WriteLine("AddJobAsync triggered.");

            var newJob = new BackupJob($"Backup {BackupJobs.Count + 1}", "/source", "/target", BackupType.Complete);
            Console.WriteLine($"Creating new job: {newJob.Name}");

            _facade.AddJob(newJob);
            Console.WriteLine($"✅ Backup job '{newJob.Name}' added successfully.");

            BackupJobs.Add(newJob);
            SelectedJob = newJob; 

            RealTimeStatus = "Job added.";
            Console.WriteLine($"✅ Job successfully added to UI: {newJob.Name}");

            await Task.CompletedTask;
        }

        private async Task ModifyJobAsync()
        {
            Console.WriteLine("ModifyJobAsync triggered.");
            if (SelectedJob == null)
            {
                Console.WriteLine("ModifyJobAsync failed: No job selected.");
                RealTimeStatus = "No job selected to modify.";
                await Task.CompletedTask;
                return;
            }

            Console.WriteLine($"Modifying job: {SelectedJob.Name}");
            SelectedJob.Name += " (Modified)";
            _facade.UpdateJob(
                BackupJobs.IndexOf(SelectedJob),
                SelectedJob.Name,
                SelectedJob.SourceDirectory,
                SelectedJob.TargetDirectory,
                SelectedJob.BackupType
            );

            RealTimeStatus = $"Job '{SelectedJob.Name}' modified successfully.";
            Console.WriteLine($"✅ Job modified: {SelectedJob.Name}");

            await Task.CompletedTask;
        }

        private async Task DeleteJobAsync()
        {
            Console.WriteLine("DeleteJobAsync triggered.");
            if (SelectedJob == null)
            {
                Console.WriteLine("DeleteJobAsync failed: No job selected.");
                RealTimeStatus = "No job selected to delete.";
                await Task.CompletedTask;
                return;
            }

            int indexToRemove = BackupJobs.IndexOf(SelectedJob);
            Console.WriteLine($"Deleting job: {SelectedJob.Name} at index {indexToRemove}");
            _facade.RemoveJob(indexToRemove);

            BackupJobs.Remove(SelectedJob);
            RealTimeStatus = $"Job '{SelectedJob.Name}' deleted successfully.";
            Console.WriteLine($"✅ Job deleted: {SelectedJob.Name}");

            await Task.CompletedTask;
        }

        private async Task ExecuteJobAsync()
        {
            Console.WriteLine("ExecuteJobAsync triggered.");
            if (SelectedJob == null)
            {
                Console.WriteLine("ExecuteJobAsync failed: No job selected.");
                RealTimeStatus = "No job selected to execute.";
                await Task.CompletedTask;
                return;
            }

            int indexToExecute = BackupJobs.IndexOf(SelectedJob);
            Console.WriteLine($"Executing job: {SelectedJob.Name} at index {indexToExecute}");
            _facade.ExecuteJobByIndex(indexToExecute);

            RealTimeStatus = $"Job '{SelectedJob.Name}' executed successfully.";
            Console.WriteLine($"✅ Job executed: {SelectedJob.Name}");

            await Task.CompletedTask;
        }

        private void OpenConfiguration()
        {
            Console.WriteLine("OpenConfiguration triggered.");
            RealTimeStatus = "Configuration opened.";
        }
    }
}
