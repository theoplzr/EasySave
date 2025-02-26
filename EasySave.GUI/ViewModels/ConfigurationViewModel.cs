using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using EasySave.GUI.Helpers;
using EasySaveLogs;
using ReactiveUI;
using System.Reactive;

namespace EasySave.GUI.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing application configuration settings (log format, directory, encryption options).
    /// </summary>
    public class ConfigurationViewModel : ReactiveObject
    {
        private string _logFormat = "XML";
        private string _businessSoftware = "Teams";
        private string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
        private ObservableCollection<string> _encryptionExtensions = new();

        private string _newExtension;
        private ObservableCollection<string> _priorityExtensions = new();
        private string _newPriorityExtension;

        /// <summary>
        /// Gets an instance of <see cref="LanguageHelper"/> for multi-language support.
        /// </summary>
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        /// <summary>
        /// Gets or sets the log format (e.g., JSON or XML).
        /// </summary>
        public string LogFormat
        {
            get => _logFormat;
            set => this.RaiseAndSetIfChanged(ref _logFormat, value);
        }

        /// <summary>
        /// Gets or sets the log directory path on the local machine.
        /// </summary>
        public string LogDirectory
        {
            get => _logDirectory;
            set => this.RaiseAndSetIfChanged(ref _logDirectory, value);
        }

        /// <summary>
        /// Gets or sets the name of the business software to monitor (which can pause operations).
        /// </summary>
        public string BusinessSoftware
        {
            get => _businessSoftware;
            set => this.RaiseAndSetIfChanged(ref _businessSoftware, value);
        }

        /// <summary>
        /// Gets or sets the collection of file extensions that should be encrypted.
        /// </summary>
        public ObservableCollection<string> EncryptionExtensions
        {
            get => _encryptionExtensions;
            set => this.RaiseAndSetIfChanged(ref _encryptionExtensions, value);
        }

        /// <summary>
        /// Holds the new extension (typing buffer) before being added to <see cref="EncryptionExtensions"/>.
        /// </summary>
        public string NewExtension
        {
            get => _newExtension;
            set => this.RaiseAndSetIfChanged(ref _newExtension, value);
        }

        /// <summary>
        /// Gets or sets the collection of priority extensions.
        /// </summary>
        public ObservableCollection<string> PriorityExtensions
        {
            get => _priorityExtensions;
            set => this.RaiseAndSetIfChanged(ref _priorityExtensions, value);
        }

        /// <summary>
        /// Holds the new extension (typing buffer) before being added to <see cref="PriorityExtensions"/>.
        /// </summary>
        public string NewPriorityExtension
        {
            get => _newPriorityExtension;
            set => this.RaiseAndSetIfChanged(ref _newPriorityExtension, value);
        }

        /// <summary>
        /// A static list of supported log formats for UI selection.
        /// </summary>
        public List<string> LogFormatOptions { get; } = new() { "JSON", "XML" };

        // Reactive Commands
        public ReactiveCommand<string, Unit> AddExtensionCommand { get; }
        public ReactiveCommand<string, Unit> RemoveExtensionCommand { get; }
        public ReactiveCommand<Window, Unit> SaveCommand { get; }
        public ReactiveCommand<Window, Unit> ChooseLogDirectoryCommand { get; }
        public ReactiveCommand<Window, Unit> CloseCommand { get; }
        public ReactiveCommand<string, Unit> AddPriorityExtensionCommand { get; }
        public ReactiveCommand<string, Unit> RemovePriorityExtensionCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationViewModel"/> class.
        /// Loads existing settings, sets up commands for adding/removing extensions, and handles saving.
        /// </summary>
        public ConfigurationViewModel()
        {
            LoadSettings();

            // Define commands
            AddExtensionCommand = ReactiveCommand.Create<string>(AddExtension);
            RemoveExtensionCommand = ReactiveCommand.Create<string>(RemoveExtension);

            SaveCommand = ReactiveCommand.Create<Window>(window =>
            {
                SaveSettings();
                // Reconfigure the logger with the newly selected format
                Logger.GetInstance(LogDirectory, LogFormat).Reconfigure(LogFormat);
                window?.Close();
            });

            ChooseLogDirectoryCommand = ReactiveCommand.CreateFromTask<Window>(ChooseLogDirectory);

            CloseCommand = ReactiveCommand.Create<Window>(window =>
            {
                window?.Close();
            });

            AddPriorityExtensionCommand = ReactiveCommand.Create<string>(extension =>
            {
                if (!string.IsNullOrWhiteSpace(extension) && !_priorityExtensions.Contains(extension))
                {
                    if (!extension.StartsWith("."))
                    {
                        extension = "." + extension;
                    }
                    PriorityExtensions.Add(extension);
                }
                // Clear the input field
                NewPriorityExtension = string.Empty;
            });

            RemovePriorityExtensionCommand = ReactiveCommand.Create<string>(extension =>
            {
                if (!string.IsNullOrEmpty(extension) && PriorityExtensions.Contains(extension))
                {
                    PriorityExtensions.Remove(extension);
                }
            });
        }

        /// <summary>
        /// Loads settings from the appsettings.GUI.json file, if available.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                const string configPath = "appsettings.GUI.json";
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ConfigurationData>(json);

                    LogFormat = config?.LogFormat;
                    BusinessSoftware = config?.BusinessSoftware;
                    LogDirectory = config?.LogDirectory
                        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
                    EncryptionExtensions = new ObservableCollection<string>(
                        config?.EncryptionExtensions ?? new List<string> { ".txt", ".docx" }
                    );
                    PriorityExtensions = new ObservableCollection<string>(
                        config?.PriorityExtensions ?? new List<string> { ".txt", ".docx" }
                    );

                    Debug.WriteLine($"{LanguageHelperInstance.LogFormatLabel} : {LogDirectory}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{LanguageHelperInstance.ErrorLoadingFiles} {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the current settings to the appsettings.GUI.json file.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var config = new ConfigurationData
                {
                    LogFormat = LogFormat,
                    BusinessSoftware = BusinessSoftware,
                    LogDirectory = LogDirectory,
                    EncryptionExtensions = EncryptionExtensions.ToList(),
                    PriorityExtensions = PriorityExtensions.ToList()
                };

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("appsettings.GUI.json", json);

                Debug.WriteLine($"{LanguageHelperInstance.ButtonSave} - {LogDirectory}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{LanguageHelperInstance.ErrorLoadingFiles} {ex.Message}");
            }
        }

        /// <summary>
        /// Adds an extension to the <see cref="EncryptionExtensions"/> list if not already present.
        /// </summary>
        /// <param name="extension">The extension to add (e.g., ".txt").</param>
        public void AddExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension) && !EncryptionExtensions.Contains(extension))
            {
                if (!extension.StartsWith("."))
                {
                    extension = "." + extension;
                }
                EncryptionExtensions.Add(extension);
            }
            NewExtension = string.Empty; // Clear the text field after adding
        }

        /// <summary>
        /// Removes an extension from the <see cref="EncryptionExtensions"/> list.
        /// </summary>
        /// <param name="extension">The extension to remove.</param>
        public void RemoveExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension) && EncryptionExtensions.Contains(extension))
            {
                EncryptionExtensions.Remove(extension);
            }
        }

        /// <summary>
        /// Opens a folder picker for selecting a new log directory.
        /// </summary>
        /// <param name="window">The owner window for the folder picker dialog.</param>
        private async Task ChooseLogDirectory(Window window)
        {
            var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                LogDirectory = folders[0].Path.LocalPath;
            }
        }
    }

    /// <summary>
    /// Represents the data structure stored in the appsettings.GUI.json file.
    /// </summary>
    public class ConfigurationData
    {
        public string LogFormat { get; set; } = "JSON";
        public string BusinessSoftware { get; set; } = "Calculator";
        public string LogDirectory { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
        public List<string> EncryptionExtensions { get; set; } = new();
        public List<string> PriorityExtensions { get; set; } = new();
    }
}
