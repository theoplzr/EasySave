using System;
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
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly EasySaveFacade _facade;
        private readonly IConfiguration _configuration;
        private readonly FileStateObserver _fileStateObserver;
        private string _businessSoftware;
        private bool _isObserverActive = false;

        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        public ObservableCollection<BackupJob> BackupJobs { get; }

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

        // Commandes
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

            OpenAddJobWindowCommand = ReactiveCommand.Create(OpenAddJobWindow);
            OpenModifyJobWindowCommand = ReactiveCommand.Create(OpenModifyJobWindow);
            OpenListAllJobWindowCommand = ReactiveCommand.Create(OpenAllJobWindow);
            DeleteJobCommand = ReactiveCommand.CreateFromTask(DeleteJobAsync);
            ExecuteAllJobsCommand = ReactiveCommand.CreateFromTask(ExecuteAllJobsAsync);
            OpenConfigurationCommand = ReactiveCommand.Create(OpenConfiguration);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
            ChangeLanguageCommand = ReactiveCommand.Create<string>(ChangeLanguage);
        }

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

        private async Task ExecuteAllJobsAsync()
        {
            if (IsBusinessSoftwareRunning())
            {
                RealTimeStatus = $"🚨 Execution blocked: {_businessSoftware} is running.";
                return;
            }

            if (!_isObserverActive)
            {
                _facade.AddObserver(_fileStateObserver);
                _isObserverActive = true;
            }

            _facade.ExecuteAllJobs();
            RealTimeStatus = "✅ All jobs executed successfully.";
            await Task.CompletedTask;
        }

        private async Task DeleteJobAsync()
        {
            if (SelectedJob == null)
            {
                RealTimeStatus = "❌ Please select a job before deleting.";
                return;
            }

            int index = BackupJobs.IndexOf(SelectedJob);
            if (index == -1)
                return;

            _facade.RemoveJob(index);
            BackupJobs.RemoveAt(index);
            SelectedJob = null;
            RealTimeStatus = $"🗑️ Job deleted.";
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

        private bool IsBusinessSoftwareRunning()
        {
            var processes = Process.GetProcesses();
            return processes.Any(p => p.ProcessName.Contains(_businessSoftware, StringComparison.OrdinalIgnoreCase));
        }

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
                    RealTimeStatus = $"✅ Job '{result.Name}' ajouté avec succès.";
                }
            }
        }

        private async void OpenModifyJobWindow()
        {
            if (SelectedJob == null)
            {
                RealTimeStatus = "❌ Please select a job before modifying.";
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
                        RealTimeStatus = $"✏️ Job '{result.Name}' modified.";
                    }
                }
            }
        }

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
