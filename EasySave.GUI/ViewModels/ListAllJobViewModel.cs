using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Avalonia.Controls;
using EasySave.Core.Models;
using EasySave.Core.Facade;
using EasySave.GUI.Helpers;
using System.Reactive.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Xml.Linq;

namespace EasySave.GUI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying and managing the list of all backup jobs.
    /// This class handles listing, selecting, and removing backup jobs.
    /// </summary>
    public class ListAllJobViewModel : ReactiveObject
    {
        private readonly Window? _window;
        private readonly EasySaveFacade _facade;
        private BackupJob? _selectedBackupJob;

        /// <summary>
        /// Gets the singleton instance of the LanguageHelper class.
        /// </summary>
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        /// <summary>
        /// Collection of backup jobs displayed in the UI.
        /// </summary>
        public ObservableCollection<FinishedBackupJob> BackupJobs { get; }

        /// <summary>
        /// Command to close the window.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Gets or sets the currently selected backup job.
        /// When a job is selected in the UI, this property is updated.
        /// </summary>
        public BackupJob? SelectedBackupJob
        {
            get => _selectedBackupJob;
            set => this.RaiseAndSetIfChanged(ref _selectedBackupJob, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListAllJobViewModel"/> class.
        /// Loads backup jobs from both JSON and XML files.
        /// </summary>
        public ListAllJobViewModel(Window window, EasySaveFacade facade)
        {
            _window = window;
            _facade = facade;
            BackupJobs = new ObservableCollection<FinishedBackupJob>();
            LoadBackupJobs();
            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        /// <summary>
        /// Loads backup jobs from both JSON and XML log files,
        /// sorted by their creation date from most recent to oldest.
        /// </summary>
        public void LoadBackupJobs()
        {
            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");

            if (!Directory.Exists(logDirectory))
            {
                Console.WriteLine($"The directory {logDirectory} does not exist.");
                return;
            }

            var logFiles = Directory.GetFiles(logDirectory, "*.json")
                .Concat(Directory.GetFiles(logDirectory, "*.xml"))
                .OrderByDescending(File.GetCreationTime) // Sort by most recent first
                .ToList();

            foreach (var file in logFiles)
            {
                try
                {
                    if (file.EndsWith(".json"))
                    {
                        LoadBackupJobsFromJson(file);
                    }
                    else if (file.EndsWith(".xml"))
                    {
                        LoadBackupJobsFromXml(file);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads backup jobs from a specific JSON file.
        /// </summary>
        private void LoadBackupJobsFromJson(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                List<FinishedBackupJob> finishedBackupJobs = ParseFinishedBackupJobs(jsonContent);
                foreach (var job in finishedBackupJobs)
                {
                    BackupJobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON file {filePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses a JSON string and converts it into a list of FinishedBackupJob objects.
        /// </summary>
        private List<FinishedBackupJob> ParseFinishedBackupJobs(string jsonContent)
        {
            try
            {
                var rawJobs = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);
                return rawJobs.Select(job => new FinishedBackupJob(
                    job.BackupName.ToString(),
                    job.SourceFilePath.ToString(),
                    job.TargetFilePath.ToString(),
                    Convert.ToInt64(job.FileSize),
                    Convert.ToInt32(job.TransferTimeMs),
                    Convert.ToInt32(job.EncryptionTimeMs),
                    job.Status.ToString(),
                    job.Level.ToString(),
                    DateTime.Parse(job.Timestamp.ToString())
                )).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
                return new List<FinishedBackupJob>();
            }
        }

        /// <summary>
        /// Loads backup jobs from a specific XML file.
        /// </summary>
        private void LoadBackupJobsFromXml(string filePath)
        {
            try
            {
                XDocument xmlDoc = XDocument.Load(filePath);
                var logEntries = xmlDoc.Descendants("LogEntry");
                foreach (var entry in logEntries)
                {
                    FinishedBackupJob finishedJob = new FinishedBackupJob(
                        entry.Element("BackupName")?.Value ?? "Unknown",
                        entry.Element("SourceFilePath")?.Value ?? "Unknown",
                        entry.Element("TargetFilePath")?.Value ?? "Unknown",
                        Convert.ToInt64(entry.Element("FileSize")?.Value ?? "0"),
                        Convert.ToInt32(entry.Element("TransferTimeMs")?.Value ?? "0"),
                        Convert.ToInt32(entry.Element("EncryptionTimeMs")?.Value ?? "0"),
                        entry.Element("Status")?.Value ?? "Unknown",
                        entry.Element("Level")?.Value ?? "Info",
                        DateTime.TryParse(entry.Element("Timestamp")?.Value, out DateTime timestamp) ? timestamp : DateTime.Now
                    );
                    BackupJobs.Add(finishedJob);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing XML file {filePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Closes the window when the Cancel button is clicked.
        /// </summary>
        private void Cancel()
        {
            _window?.Close();
        }
    }
}
