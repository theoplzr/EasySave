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

namespace EasySave.GUI.ViewModels
{
    /// <summary>
    /// ViewModel for displaying and managing the list of all backup jobs.
    /// This class handles listing, selecting, and removing backup jobs.
    /// </summary>
    public class ListAllJobViewModel : ReactiveObject
    {
        /// <summary>
        /// Reference to the window associated with this ViewModel.
        /// Used to close the window when needed.
        /// </summary>
        private readonly Window? _window;
        /// <summary>
        /// Facade providing access to the backup jobs management logic.
        /// </summary>
        private readonly EasySaveFacade _facade;
        /// <summary>
        /// The currently selected backup job in the UI.
        /// </summary>
        private BackupJob? _selectedBackupJob;
        /// <summary>
        /// Gets the singleton instance of the LanguageHelper class.
        /// </summary>
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;
        /// <summary>
        /// Collection of backup jobs to be displayed in the UI.
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
        /// </summary>
        /// <param name="window">The window associated with this ViewModel.</param>
        /// <param name="facade">The facade providing access to backup job operations.</param>
        public ListAllJobViewModel(Window window, EasySaveFacade facade)
        {
            _window = window;
            _facade = facade;

            string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
            BackupJobs = new ObservableCollection<FinishedBackupJob>();
            LoadBackupJobsFromJson(); // Load the jobs

            CancelCommand = ReactiveCommand.Create(Cancel);
        }

        /// <summary>
        /// Loads backup jobs from JSON files stored in the "Logs" directory.
        /// It reads all JSON files, deserializes them into FinishedBackupJob objects,
        /// and adds them to the BackupJobs collection while avoiding duplicates.
        /// </summary>
        public void LoadBackupJobsFromJson()
        {
            // Define the logs directory path
            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");

            // Check if the directory exists
            if (!Directory.Exists(logDirectory))
            {
                Console.WriteLine($"The directory {logDirectory} does not exist.");
                return;
            }

            // Retrieve all JSON files from the directory
            var jsonFiles = Directory.GetFiles(logDirectory, "*.json");

            foreach (var file in jsonFiles)
            {
                try
                {
                    // Read the content of the JSON file
                    string jsonContent = File.ReadAllText(file);

                    // Deserialize the JSON into a list of FinishedBackupJob objects
                    List<FinishedBackupJob> finishedBackupJobs = ParseFinishedBackupJobs(jsonContent);

                    // If the parsed jobs are not null, process them
                    if (finishedBackupJobs != null)
                    {
                        foreach (var job in finishedBackupJobs)
                        {
                            // Check if the job already exists to prevent duplicates
                            if (!BackupJobs.Any(b => b.Name == job.Name && 
                                                    b.SourceDirectory == job.SourceDirectory && 
                                                    b.TargetDirectory == job.TargetDirectory))
                            {
                                BackupJobs.Add(job);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Deserialize JSON and convert it into a list of FinishedBackupJob objects.
        /// </summary>
        public List<FinishedBackupJob> ParseFinishedBackupJobs(string jsonContent)
        {
            try
            {
                // Deserialize JSON into a list of anonymous objects
                var rawJobs = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);

                // Create a list to store the converted objects
                List<FinishedBackupJob> finishedJobs = new List<FinishedBackupJob>();

                foreach (var job in rawJobs)
                {
                    FinishedBackupJob finishedJob = new FinishedBackupJob(
                        job.BackupName.ToString(),
                        job.SourceFilePath.ToString(),
                        job.TargetFilePath.ToString(),
                        Convert.ToInt64(job.FileSize),
                        Convert.ToInt32(job.TransferTimeMs),
                        Convert.ToInt32(job.EncryptionTimeMs),
                        job.Status.ToString(),
                        Convert.ToInt32(job.Level),
                        DateTime.Parse(job.Timestamp.ToString()) 
                    );

                    finishedJobs.Add(finishedJob);
                }

                return finishedJobs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error parsing JSON: {ex.Message}");
                return new List<FinishedBackupJob>(); // Return empty list if error
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
