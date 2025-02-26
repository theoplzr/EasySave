using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using EasySave.Core.Facade;
using EasySave.Core.Models;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;
using EasySave.GUI.Helpers;
using EasySave.GUI.Observers;
using EasySave.GUI.Views;
using Microsoft.Extensions.Configuration;
using ReactiveUI;

namespace EasySave.GUI.ViewModels
{
    /// <summary>
    /// The primary ViewModel for the main application window.
    /// Manages backup jobs, configuration, and real-time execution status.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly EasySaveFacade _facade;
        private readonly IConfiguration _configuration;
        private readonly FileStateObserver _fileStateObserver;

        private string _businessSoftware;
        private bool _isObserverActive;
        private BackupJob? _selectedJob;
        private string _realTimeStatus = "Idle";

        /// <summary>
        /// Gets the instance of <see cref="LanguageHelper"/>.
        /// </summary>
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        /// <summary>
        /// Gets the collection of backup jobs displayed in the UI.
        /// </summary>
        public ObservableCollection<BackupJob> BackupJobs { get; }

        /// <summary>
        /// Gets or sets the selected backup job in the UI.
        /// </summary>
        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set => this.RaiseAndSetIfChanged(ref _selectedJob, value);
        }

        /// <summary>
        /// Gets or sets a message indicating the real-time backup execution status.
        /// </summary>
        public string RealTimeStatus
        {
            get => _realTimeStatus;
            set => this.RaiseAndSetIfChanged(ref _realTimeStatus, value);
        }

        #region Commands

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

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class,
        /// setting up the facade, observers, and commands.
        /// </summary>
        public MainWindowViewModel()
        {
            // Load configuration from appsettings.GUI.json
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.GUI.json", optional: true, reloadOnChange: true)
                .Build();

            _businessSoftware = _configuration["BusinessSoftware"] ?? "Calculator";

            _facade = new EasySaveFacade(
                new JsonBackupJobRepository("backup_jobs.json"),
                "Logs",
                null,
                _configuration
            );

            _fileStateObserver = new FileStateObserver("state.json");

            // Load and store the jobs in an observable collection
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

            // Observer for UI updates
            _facade.AddObserver(new UiObserver(this));

            // Initialize commands
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

            OpenAddJobWindowCommand = ReactiveCommand.Create(OpenAddJobWindow);
            OpenModifyJobWindowCommand = ReactiveCommand.Create(OpenModifyJobWindow);
            OpenListAllJobWindowCommand = ReactiveCommand.Create(OpenAllJobWindow);
            DeleteJobCommand = ReactiveCommand.CreateFromTask(DeleteJobAsync);
            ExecuteAllJobsCommand = ReactiveCommand.CreateFromTask(ExecuteAllJobsAsync);
            OpenConfigurationCommand = ReactiveCommand.Create(OpenConfiguration);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
            ChangeLanguageCommand = ReactiveCommand.Create<string>(ChangeLanguage);
        }

        #region Event Handlers

        /// <summary>
        /// Handles property changes on backup jobs, ensuring only one job is selected at a time.
        /// </summary>
        private void Job_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is BackupJob job && e.PropertyName == nameof(BackupJob.IsSelected))
            {
                // If a job is selected, unselect all other jobs
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

        #endregion

        #region Command Methods

        /// <summary>
        /// Asynchronously executes all backup jobs in parallel, updating the real-time status.
        /// </summary>
        private async Task ExecuteAllJobsAsync()
        {
            if (IsBusinessSoftwareRunning())
            {
                RealTimeStatus = $"{LanguageHelperInstance.GetMessage("ExecutionBlocked")} {_businessSoftware} {LanguageHelperInstance.GetMessage("IsRunning")}";
                return;
            }

            RealTimeStatus = LanguageHelperInstance.GetMessage("ExecutionRunning");

            if (!_isObserverActive)
            {
                _facade.AddObserver(_fileStateObserver);
                _isObserverActive = true;
            }

            try
            {
                // Run execution on a separate task
                Task.Run(async () =>
                {
                    _facade.ExecuteAllJobs(); // Start the backups

                    // Monitor the facade status while running
                    while (_facade.GetStatus() != "finished")
                    {
                        switch (_facade.GetStatus())
                        {
                            case "paused":
                                RealTimeStatus = LanguageHelperInstance.GetMessage("ExecutionPaused");
                                break;
                            case "running":
                                RealTimeStatus = LanguageHelperInstance.GetMessage("ExecutionRunning");
                                break;
                            default:
                                break;
                        }

                        await Task.Delay(1000); // Wait 1 second before checking again
                    }
                });

                RealTimeStatus = LanguageHelperInstance.GetMessage("ExecutionRunning");
            }
            catch (Exception)
            {
                RealTimeStatus = LanguageHelperInstance.GetMessage("ExecutionFailed");
            }
        }

        /// <summary>
        /// Removes the selected job from the facade and UI list.
        /// </summary>
        private async Task DeleteJobAsync()
        {
            if (SelectedJob == null)
            {
                RealTimeStatus = LanguageHelperInstance.GetMessage("PleaseSelectJobForDeletion");
                return;
            }

            int index = BackupJobs.IndexOf(SelectedJob);
            if (index == -1) return;

            _facade.RemoveJob(index);
            BackupJobs.RemoveAt(index);
            SelectedJob = null;
            RealTimeStatus = LanguageHelperInstance.GetMessage("JobDeleted");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Opens the configuration window, allowing the user to modify log or encryption settings.
        /// </summary>
        private void OpenConfiguration()
        {
            var configWindow = new ConfigurationWindow();
            configWindow.Show();
        }

        /// <summary>
        /// Changes the current language of the application by updating <see cref="LanguageHelper"/>.
        /// </summary>
        /// <param name="languageCode">The language code (e.g., "en" or "fr").</param>
        private void ChangeLanguage(string languageCode)
        {
            LanguageHelper.Instance.SetLanguage(languageCode);
        }

        /// <summary>
        /// Checks whether the monitored business software is running.
        /// If it is running, backup operations should be paused or blocked.
        /// </summary>
        private bool IsBusinessSoftwareRunning()
        {
            var processes = Process.GetProcesses();
            return processes.Any(p => p.ProcessName.Contains(_businessSoftware, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Opens a form to create a new <see cref="BackupJob"/>.
        /// </summary>
        private async void OpenAddJobWindow()
        {
            var jobWindow = new JobFormWindow();
            var jobViewModel = new JobFormViewModel(jobWindow);
            jobWindow.DataContext = jobViewModel;

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
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
        /// Opens a form to modify the currently selected <see cref="BackupJob"/>.
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

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
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
        /// Opens a new window displaying all existing backup jobs from logs.
        /// </summary>
        private async void OpenAllJobWindow()
        {
            var listWindow = new ListAllJobWindow();
            var listViewModel = new ListAllJobViewModel(listWindow, _facade);
            listWindow.DataContext = listViewModel;

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
            {
                await listWindow.ShowDialog(desktop.MainWindow);
            }
        }

        #endregion
    }
}
