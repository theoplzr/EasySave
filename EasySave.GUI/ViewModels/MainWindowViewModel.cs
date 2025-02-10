using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using EasySave.Core.Models;
using EasySave.Core.Facade;
using EasySave.GUI.Utils;
using EasySave.GUI.Views;
using EasySave.Core.Repositories;
using Microsoft.Extensions.Configuration;
using System.IO;

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
            // Scheduler configuré pour le thread principal
            RxApp.MainThreadScheduler = AvaloniaDispatcherScheduler.Instance;
        }

        public MainWindowViewModel()
        {
            Console.WriteLine("Initializing MainWindowViewModel...");

            // Charger la configuration depuis le fichier appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Console.WriteLine("Configuration loaded.");

            // Initialisation du facade
            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("backup_jobs.json"),
                "Logs",
                null,
                configuration
            );

            Console.WriteLine("Facade initialized.");

            // Charger les travaux de sauvegarde
            BackupJobs = new ObservableCollection<BackupJob>(_facade.ListBackupJobs());
            Console.WriteLine($"Loaded {BackupJobs.Count} backup jobs into the DataGrid.");
            RealTimeStatus = "Idle";

            // Initialiser les commandes
            AddJobCommand = ReactiveCommand.CreateFromTask(AddJobAsync);
            ModifyJobCommand = ReactiveCommand.CreateFromTask(ModifyJobAsync);
            DeleteJobCommand = ReactiveCommand.CreateFromTask(DeleteJobAsync);
            ExecuteJobCommand = ReactiveCommand.CreateFromTask(ExecuteJobAsync);
            OpenConfigurationCommand = ReactiveCommand.Create(OpenConfiguration);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
        }

        private void OpenConfiguration()
        {
            try
            {
                // Vérifiez si ConfigurationWindow est défini correctement
                var configWindow = new EasySave.GUI.Views.ConfigurationWindow();
                configWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening ConfigurationWindow: {ex.Message}");
            }
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
    }
}
