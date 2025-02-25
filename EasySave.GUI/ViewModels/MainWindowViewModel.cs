﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using EasySave.Core.Models;
using EasySave.Core.Facade;
using EasySave.Core.Observers;
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
    /// <summary>
    /// ViewModel for the main application window. 
    /// Manages backup jobs, configuration, and execution commands.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly EasySaveFacade _facade;
        private readonly IConfiguration _configuration;
        private readonly FileStateObserver _fileStateObserver;
        private string _businessSoftware;
        private bool _isObserverActive = false;

        /// <summary>
        /// Gets the instance of the language helper.
        /// </summary>
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        /// <summary>
        /// Collection of backup jobs displayed in the UI.
        /// </summary>
        public ObservableCollection<BackupJob> BackupJobs { get; }

        private BackupJob? _selectedJob;
        /// <summary>
        /// Gets or sets the selected backup job.
        /// </summary>
        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set => this.RaiseAndSetIfChanged(ref _selectedJob, value);
        }

        private string _realTimeStatus = "Idle";
        /// <summary>
        /// Gets or sets the real-time execution status.
        /// </summary>
        public string RealTimeStatus
        {
            get => _realTimeStatus;
            set => this.RaiseAndSetIfChanged(ref _realTimeStatus, value);
        }

        // Commandes
        public ReactiveCommand<Unit, Unit> OpenAddJobWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenModifyJobWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenListAllJobWindowCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteJobCommand { get; }
        public ReactiveCommand<Unit, Unit> ExecuteAllJobsCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenConfigurationCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<string, Unit> ChangeLanguageCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseJobCommand { get; }
        public ReactiveCommand<Unit, Unit> ResumeJobCommand { get; }
        public ReactiveCommand<Unit, Unit> StopJobCommand { get; }

        /// <summary>
        /// Initializes the main window ViewModel.
        /// Loads configuration settings and backup jobs.
        /// </summary>
        public MainWindowViewModel()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.GUI.json", optional: true, reloadOnChange: true)
                .Build();

            _businessSoftware = _configuration["BusinessSoftware"];

            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("backup_jobs.json"),
                "Logs",
                null,
                _configuration
            );

            _fileStateObserver = new FileStateObserver("state.json");

            var jobs = _facade.ListBackupJobs();
            for (int i = 0; i < jobs.Count; i++)
            {
                jobs[i].Ordinal = i;
            }

            BackupJobs = new ObservableCollection<BackupJob>(jobs);

            foreach (var job in BackupJobs)
            {
                job.PropertyChanged += Job_PropertyChanged;
            }

            PauseJobCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedJob != null)
                {
                    _facade.PauseJob(SelectedJob.Id);
                    RealTimeStatus = LanguageHelperInstance.GetMessage("JobPaused");
                }
            });

            ResumeJobCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedJob != null)
                {
                    _facade.ResumeJob(SelectedJob.Id);
                    RealTimeStatus = LanguageHelperInstance.GetMessage("JobResumed");
                }
            });

            StopJobCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedJob != null)
                {
                    _facade.StopJob(SelectedJob.Id);
                    RealTimeStatus = LanguageHelperInstance.GetMessage("JobStopped");
                }
            });

            _facade.AddObserver(new UiObserver(this));

            OpenAddJobWindowCommand = ReactiveCommand.Create(OpenAddJobWindow);
            OpenModifyJobWindowCommand = ReactiveCommand.Create(OpenModifyJobWindow);
            OpenListAllJobWindowCommand = ReactiveCommand.Create(OpenAllJobWindow);
            DeleteJobCommand = ReactiveCommand.CreateFromTask(DeleteJobAsync);
            ExecuteAllJobsCommand = ReactiveCommand.CreateFromTask(ExecuteAllJobsAsync);
            OpenConfigurationCommand = ReactiveCommand.Create(OpenConfiguration);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
            ChangeLanguageCommand = ReactiveCommand.Create<string>(ChangeLanguage);
        }

        /// <summary>
        /// Handles the property changed event for backup jobs.
        /// Ensures only one job is selected at a time.
        /// </summary>
        private void Job_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is BackupJob job && e.PropertyName == nameof(BackupJob.IsSelected))
            {
                if (job.IsSelected)
                {
                    foreach (var otherJob in BackupJobs)
                    {
                        if (!ReferenceEquals(otherJob, job) && otherJob.IsSelected)
                        {
                            otherJob.IsSelected = false;
                        }
                    }
                    SelectedJob = job;
                }
                else if (!job.IsSelected && SelectedJob == job)
                {
                    SelectedJob = null;
                }
            }
        }

        /// <summary>
        /// Executes all backup jobs asynchronously.
        /// </summary>
        private async Task ExecuteAllJobsAsync()
        {
            if (IsBusinessSoftwareRunning())
            {
                RealTimeStatus = $"{LanguageHelperInstance.GetMessage("ExecutionBlocked")} {_businessSoftware} {LanguageHelperInstance.GetMessage("IsRunning")}";
            }
            RealTimeStatus = $"{LanguageHelperInstance.GetMessage("ExecutionRunning")}";

            if (!_isObserverActive)
            {
                _facade.AddObserver(_fileStateObserver);
                _isObserverActive = true;
            }

            try
            {
                // Lancer l'exécution en parallèle
                Task.Run(async () =>
                {
                    _facade.ExecuteAllJobs(); // Lancer l'exécution des sauvegardes

                    // Pendant que l'exécution est en cours, surveiller son état
                    while (_facade.GetStatus() != "finished")
                    {
                        switch (_facade.GetStatus())
                        {
                            case "paused":
                                RealTimeStatus = $"{LanguageHelperInstance.GetMessage("ExecutionPaused")}";
                                break;
                            case "running":
                                RealTimeStatus = $"{LanguageHelperInstance.GetMessage("ExecutionRunning")}";
                                break;
                            default:
                                break;
                        }

                        await Task.Delay(1000); // Attendre 1 seconde avant de vérifier à nouveau
                    }
                });
                
                RealTimeStatus = $"{LanguageHelperInstance.GetMessage("ExecutionRunning")}";
            }
            catch (Exception ex)
            {
                RealTimeStatus = $"{LanguageHelperInstance.GetMessage("ExecutionFailed")}";
            }

        }

        /// <summary>
        /// Delete jobs asynchronously.
        /// </summary>
        private async Task DeleteJobAsync()
        {
            if (SelectedJob == null)
            {
                RealTimeStatus = LanguageHelperInstance.GetMessage("PleaseSelectJobForDeletion");
                return;
            }

            int index = BackupJobs.IndexOf(SelectedJob);
            if (index == -1)
                return;

            _facade.RemoveJob(index);
            BackupJobs.RemoveAt(index);
            SelectedJob = null;
            RealTimeStatus = LanguageHelperInstance.GetMessage("JobDeleted");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Opens the configuration window.
        /// </summary>
        private void OpenConfiguration()
        {
            var configWindow = new ConfigurationWindow();
            configWindow.Show();
        }

        /// <summary>
        /// Change the language.
        /// </summary>
        private void ChangeLanguage(string languageCode)
        {
            LanguageHelper.Instance.SetLanguage(languageCode);
        }

        /// <summary>
        /// Checks if the business software is currently running.
        /// </summary>
        private bool IsBusinessSoftwareRunning()
        {
            var processes = Process.GetProcesses();
            return processes.Any(p => p.ProcessName.Contains(_businessSoftware, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Opens the add job window.
        /// </summary>
        private async void OpenAddJobWindow()
        {
            var jobWindow = new JobFormWindow();
            var jobViewModel = new JobFormViewModel(jobWindow);
            jobWindow.DataContext = jobViewModel;

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var result = await jobWindow.ShowDialog<BackupJob>(desktop.MainWindow);
                if (result != null)
                {
                    result.Ordinal = BackupJobs.Count;
                    _facade.AddJob(result);
                    result.PropertyChanged += Job_PropertyChanged;
                    BackupJobs.Add(result);
                    RealTimeStatus = string.Format(LanguageHelperInstance.GetMessage("JobAdded"), result.Name);
                }
            }
        }

        /// <summary>
        /// Opens the modify job window.
        /// </summary>
        private async void OpenModifyJobWindow()
        {
            if (SelectedJob == null)
            {
                RealTimeStatus = LanguageHelperInstance.GetMessage("PleaseSelectJobForModification");
                return;
            }

            var jobWindow = new JobFormWindow();
            var jobViewModel = new JobFormViewModel(jobWindow, SelectedJob);
            jobWindow.DataContext = jobViewModel;

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var result = await jobWindow.ShowDialog<BackupJob>(desktop.MainWindow);
                if (result != null)
                {
                    int index = BackupJobs.IndexOf(SelectedJob);
                    if (index != -1)
                    {
                        _facade.UpdateJob(index, result.Name, result.SourceDirectory, result.TargetDirectory, result.BackupType);
                        BackupJobs[index] = result;
                        RealTimeStatus = string.Format(LanguageHelperInstance.GetMessage("JobModified"), result.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Opens the list all jobs window.
        /// </summary>
        private async void OpenAllJobWindow()
        {
            var listWindow = new ListAllJobWindow();
            var listViewModel = new ListAllJobViewModel(listWindow, _facade);
            listWindow.DataContext = listViewModel;

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                await listWindow.ShowDialog(desktop.MainWindow);
            }
        }
    }
}
