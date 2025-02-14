using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using EasySave.Core.Models;
using EasySave.Core.Facade;
using EasySave.GUI.Views;
using EasySave.Core.Repositories;
using EasySave.GUI.Observers;
using EasySave.GUI.Helpers;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace EasySave.GUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly EasySaveFacade _facade;
        private readonly IConfiguration _configuration;
        private string _businessSoftware;

        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        public ObservableCollection<BackupJob> BackupJobs { get; }

        private ObservableCollection<BackupState> _backupStates;
        public ObservableCollection<BackupState> BackupStates
        {
            get => _backupStates;
            set => this.RaiseAndSetIfChanged(ref _backupStates, value);
        }

        private BackupJob? _selectedJob;
        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set => this.RaiseAndSetIfChanged(ref _selectedJob, value);
        }

        private string _realTimeStatus = "Idle";
        public string RealTimeStatus
        {
            get => _realTimeStatus;
            set => this.RaiseAndSetIfChanged(ref _realTimeStatus, value);
        }

        // Commandes mises à jour
        public ReactiveCommand<Unit, Unit> OpenAddJobWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenModifyJobWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenListAllJobWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteJobCommand { get; }
        public ReactiveCommand<Unit, Unit> ExecuteAllJobsCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenConfigurationCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<string, Unit> ChangeLanguageCommand { get; }

        public MainWindowViewModel()
        {
            // Charger la configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            _businessSoftware = _configuration["BusinessSoftware"] ?? "TextEdit"; 

            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("backup_jobs.json"),
                "Logs",
                null,
                _configuration
            );

            // Récupération de la liste des jobs depuis le _facade
            var jobs = _facade.ListBackupJobs();

            // Pour chaque job, assigner un ordinal basé sur sa position dans la liste
            for (int i = 0; i < jobs.Count; i++)
            {
                jobs[i].Ordinal = i;  // Assurez-vous que BackupJob possède une propriété Ordinal
            }

            BackupJobs = new ObservableCollection<BackupJob>(jobs);

            BackupStates = new ObservableCollection<BackupState>();

            var uiObserver = new UIStateObserver(BackupStates);
            _facade.AddObserver(uiObserver); 

            // Initialiser les commandes
            OpenAddJobWindowCommand = ReactiveCommand.Create(OpenAddJobWindow);
            OpenModifyJobWindowCommand = ReactiveCommand.Create(OpenModifyJobWindow);
            OpenListAllJobWindowCommand = ReactiveCommand.Create(OpenAllJobWindow);
            DeleteJobCommand = ReactiveCommand.CreateFromTask(DeleteJobAsync);
            ExecuteAllJobsCommand = ReactiveCommand.CreateFromTask(ExecuteAllJobsAsync);
            OpenConfigurationCommand = ReactiveCommand.Create(OpenConfiguration);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
            ChangeLanguageCommand = ReactiveCommand.Create<string>(ChangeLanguage);
        }

        private async void OpenAddJobWindow()
        {
            var jobWindow = new JobFormWindow();
            var jobViewModel = new JobFormViewModel(jobWindow);
            jobWindow.DataContext = jobViewModel;

            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null)
            {
                var result = await jobWindow.ShowDialog<BackupJob>(mainWindow); 
                if (result != null)
                {
                    // Assigner l'ordinal en fonction du nombre actuel de jobs
                    result.Ordinal = BackupJobs.Count;  
                    _facade.AddJob(result);
                    BackupJobs.Add(result);

                    RealTimeStatus = $"✅ Job '{result.Name}' ajouté avec succès.";
                }
                else
                {
                    RealTimeStatus = "⚠️ Ajout de job annulé.";
                }
            }
        }

        private async void OpenModifyJobWindow()
        {
            if (SelectedJob == null) 
            {
                RealTimeStatus = "❌ No job selected to modify.";
                return;
            }

            Console.WriteLine($"🔍 Modification en cours pour le job : {SelectedJob.Name}");

            var jobWindow = new JobFormWindow();
            var jobViewModel = new JobFormViewModel(jobWindow, SelectedJob);
            jobWindow.DataContext = jobViewModel;

            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null)
            {
                var result = await jobWindow.ShowDialog<BackupJob>(mainWindow); 
                if (result != null)
                {
                    Console.WriteLine($"✅ Job modifié : {result.Name}");

                    int index = BackupJobs.IndexOf(SelectedJob);
                    if (index != -1)
                    {
                        _facade.UpdateJob(index, result.Name, result.SourceDirectory, result.TargetDirectory, result.BackupType);
                        BackupJobs[index] = result;
                        RealTimeStatus = $"✏️ Job '{result.Name}' modified.";
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ Aucune modification effectuée.");
                }
            }
        }

        private async void OpenAllJobWindow()
        {
            var listWindow = new ListAllJobWindow();
            var listViewModel = new ListAllJobViewModel(listWindow, _facade);
            listWindow.DataContext = listViewModel;

            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null)
            {
                await listWindow.ShowDialog(mainWindow);
            }
        }

        
        private async Task DeleteJobAsync()
        {
            if (SelectedJob == null)
            {
                RealTimeStatus = "❌ No job selected to delete.";
                Console.WriteLine("❌ Aucun job sélectionné pour suppression.");
                return;
            }

            Console.WriteLine($"🗑️ Suppression du job : {SelectedJob.Name}");

            int index = BackupJobs.IndexOf(SelectedJob);
            if (index == -1)
            {
                Console.WriteLine("⚠️ Job introuvable dans la liste.");
                return;
            }

            _facade.RemoveJob(index);
            BackupJobs.RemoveAt(index);
            RealTimeStatus = $"🗑️ Job '{SelectedJob.Name}' deleted.";
            
            Console.WriteLine($"✅ Job supprimé : {SelectedJob.Name}");

            await Task.CompletedTask;
        }


        private async Task ExecuteAllJobsAsync()
        {
            if (IsBusinessSoftwareRunning())
            {
                RealTimeStatus = $"🚨 Execution blocked: {_businessSoftware} is running.";
                return;
            }

            _facade.ExecuteAllJobs();
            RealTimeStatus = "✅ All jobs executed successfully.";
            await Task.CompletedTask;
        }

        private void OpenConfiguration()
        {
            var configWindow = new ConfigurationWindow();
            configWindow.Show();
        }

        private void ChangeLanguage(string languageCode)
        {
            LanguageHelper.Instance.SetLanguage(languageCode);
        }

        /// <summary>
        /// Vérifie si le logiciel métier est en cours d'exécution
        /// </summary>
        private bool IsBusinessSoftwareRunning()
        {
            var processes = Process.GetProcesses();
            return processes.Any(p => p.ProcessName.Contains(_businessSoftware, StringComparison.OrdinalIgnoreCase));
        }
    }
}
