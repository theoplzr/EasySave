using Avalonia.Controls;
using EasySave.Core.Facade;
using EasySave.Core.Models;
using EasySave.GUI.Helpers;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Xml.Linq;

namespace EasySave.GUI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying and managing a list of all backup jobs.
    /// Loads job logs from JSON and XML files, displaying them in a UI collection.
    /// </summary>
    public class ListAllJobViewModel : ReactiveObject
    {
        private readonly Window? _window;
        private readonly EasySaveFacade _facade;
        private BackupJob? _selectedBackupJob;

        /// <summary>
        /// Provides access to localized messages.
        /// </summary>
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        /// <summary>
        /// An observable collection of <see cref="FinishedBackupJob"/> records for UI display.
        /// </summary>
        public ObservableCollection<FinishedBackupJob> BackupJobs { get; }

        /// <summary>
        /// Command to close the window and return to the previous view.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Gets or sets the currently selected <see cref="BackupJob"/>.
        /// </summary>
        public BackupJob? SelectedBackupJob
        {
            get => _selectedBackupJob;
            set => this.RaiseAndSetIfChanged(ref _selectedBackupJob, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListAllJobViewModel"/> class,
        /// retrieving and displaying all backup job logs.
        /// </summary>
        /// <param name="window">The parent window context.</param>
        /// <param name="facade">The <see cref="EasySaveFacade"/> for managing backup operations.</param>
        public ListAllJobViewModel(Window window, EasySaveFacade facade)
        {
            _window = window;
            _facade = facade;
            BackupJobs = new ObservableCollection<FinishedBackupJob>();
            CancelCommand = ReactiveCommand.Create(Cancel);

            LoadBackupJobs();
        }

        /// <summary>
        /// Loads backup job logs from JSON and XML files, located in the default log directory.
        /// Sorts them by file creation time (most recent first).
        /// </summary>
        public void LoadBackupJobs()
        {
            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Console.WriteLine($"Directory '{logDirectory}' does not exist.");
                return;
            }

            var logFiles = Directory.GetFiles(logDirectory, "*.json")
                .Concat(Directory.GetFiles(logDirectory, "*.xml"))
                .OrderByDescending(File.GetCreationTime)
                .ToList();

            foreach (var file in logFiles)
            {
                try
                {
                    if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadBackupJobsFromJson(file);
                    }
                    else if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadBackupJobsFromXml(file);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file '{file}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Reads backup job logs from a JSON file and adds them to the <see cref="BackupJobs"/> collection.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        private void LoadBackupJobsFromJson(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                var finishedJobs = ParseFinishedBackupJobs(jsonContent);
                foreach (var job in finishedJobs)
                {
                    BackupJobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON file '{filePath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Parses JSON content into a list of <see cref="FinishedBackupJob"/> records.
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
        /// Reads backup job logs from an XML file and adds them to the <see cref="BackupJobs"/> collection.
        /// </summary>
        /// <param name="filePath">The path to the XML file.</param>
        private void LoadBackupJobsFromXml(string filePath)
        {
            try
            {
                XDocument xmlDoc = XDocument.Load(filePath);
                var logEntries = xmlDoc.Descendants("LogEntry");

                foreach (var entry in logEntries)
                {
                    var finishedJob = new FinishedBackupJob(
                        entry.Element("BackupName")?.Value ?? "Unknown",
                        entry.Element("SourceFilePath")?.Value ?? "Unknown",
                        entry.Element("TargetFilePath")?.Value ?? "Unknown",
                        Convert.ToInt64(entry.Element("FileSize")?.Value ?? "0"),
                        Convert.ToInt32(entry.Element("TransferTimeMs")?.Value ?? "0"),
                        Convert.ToInt32(entry.Element("EncryptionTimeMs")?.Value ?? "0"),
                        entry.Element("Status")?.Value ?? "Unknown",
                        entry.Element("Level")?.Value ?? "Info",
                        DateTime.TryParse(entry.Element("Timestamp")?.Value, out DateTime timestamp)
                            ? timestamp
                            : DateTime.Now
                    );
                    BackupJobs.Add(finishedJob);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing XML file '{filePath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Closes the current window, returning to the previous UI context.
        /// </summary>
        private void Cancel()
        {
            _window?.Close();
        }
    }
}
