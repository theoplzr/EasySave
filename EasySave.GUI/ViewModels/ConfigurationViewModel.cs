using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using EasySave.GUI.Helpers;
using EasySaveLogs;

namespace EasySave.GUI.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing application configuration settings.
    /// </summary>
    public class ConfigurationViewModel : ReactiveObject
    {
        private string _logFormat = "XML";
        private string _businessSoftware = "Calculator";
        private string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
        private ObservableCollection<string> _encryptionExtensions = new();

        /// <summary>
        /// Instance of the language helper for multi-language support.
        /// </summary>
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        /// <summary>
        /// Gets or sets the log format (JSON or XML).
        /// </summary>
        public string LogFormat
        {
            get => _logFormat;
            set => this.RaiseAndSetIfChanged(ref _logFormat, value);
        }

        /// <summary>
        /// Gets or sets the log directory path.
        /// </summary>
        public string LogDirectory
        {
            get => _logDirectory;
            set => this.RaiseAndSetIfChanged(ref _logDirectory, value);
        }

        /// <summary>
        /// Gets or sets the name of the business software that may pause operations.
        /// </summary>
        public string BusinessSoftware
        {
            get => _businessSoftware;
            set => this.RaiseAndSetIfChanged(ref _businessSoftware, value);
        }

        /// <summary>
        /// Gets or sets the list of file extensions that require encryption.
        /// </summary>
        public ObservableCollection<string> EncryptionExtensions
        {
            get => _encryptionExtensions;
            set => this.RaiseAndSetIfChanged(ref _encryptionExtensions, value);
        }

        private string _newPriorityExtension;
        public string NewPriorityExtension
        {
            get => _newPriorityExtension;
            set => this.RaiseAndSetIfChanged(ref _newPriorityExtension, value);
        }

        private ObservableCollection<string> _priorityExtensions = new();
        public ObservableCollection<string> PriorityExtensions
        {
            get => _priorityExtensions;
            set => this.RaiseAndSetIfChanged(ref _priorityExtensions, value);
        }

        public List<string> LogFormatOptions { get; } = new() { "JSON", "XML" };

        public ReactiveCommand<string, Unit> AddExtensionCommand { get; }
        public ReactiveCommand<string, Unit> RemoveExtensionCommand { get; }
        public ReactiveCommand<Window, Unit> SaveCommand { get; }
        public ReactiveCommand<Window, Unit> ChooseLogDirectoryCommand { get; }
        public ReactiveCommand<Window, Unit> CloseCommand { get; }
        public ReactiveCommand<string, Unit> AddPriorityExtensionCommand { get; }
        public ReactiveCommand<string, Unit> RemovePriorityExtensionCommand { get; }

        /// <summary>
        /// Initializes a new instance of the ConfigurationViewModel class.
        /// </summary>
        public ConfigurationViewModel()
        {
            LoadSettings();
            AddExtensionCommand = ReactiveCommand.Create<string>(AddExtension);
            RemoveExtensionCommand = ReactiveCommand.Create<string>(RemoveExtension);
            SaveCommand = ReactiveCommand.Create<Window>(window =>
            {
                SaveSettings();
                // Reconfigure le Logger avec le nouveau format choisi
                Logger.GetInstance(LogDirectory, LogFormat).Reconfigure(LogFormat);
                window?.Close();
            });
            ChooseLogDirectoryCommand = ReactiveCommand.CreateFromTask<Window>(ChooseLogDirectory);
            
            // Correction de CloseCommand avec vérification de null
            CloseCommand = ReactiveCommand.Create<Window>(window =>
            {
                if (window != null)
                {
                    window.Close();
                }
            });

            AddPriorityExtensionCommand = ReactiveCommand.Create<string>(extension =>
            {
                if (!string.IsNullOrWhiteSpace(extension) && !_priorityExtensions.Contains(extension))
                {
                    PriorityExtensions.Add(extension);
                }
                
                // Clear the input field after adding
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
        /// Loads configuration settings from a file.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                string configPath = "appsettings.GUI.json";
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ConfigurationData>(json);

                    LogFormat = config?.LogFormat;
                    BusinessSoftware = config?.BusinessSoftware;
                    LogDirectory = config?.LogDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
                    EncryptionExtensions = new ObservableCollection<string>(config?.EncryptionExtensions ?? new List<string> { ".txt", ".docx" });
                    PriorityExtensions = new ObservableCollection<string>(config?.PriorityExtensions ?? new List<string> { ".txt", ".docx" });

                    Debug.WriteLine($"{LanguageHelperInstance.LogFormatLabel} : {LogDirectory}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{LanguageHelperInstance.ErrorLoadingFiles} {ex.Message}");
            }
        }

        /// <summary>
        /// Saves configuration settings to a file.
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
        /// Add an extension to encrypt.
        /// </summary>
        public void AddExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension) && !EncryptionExtensions.Contains(extension))
            {
                EncryptionExtensions.Add(extension);
            }
        }

        /// <summary>
        /// Remove an extension to encrypt.
        /// </summary>
        public void RemoveExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension) && EncryptionExtensions.Contains(extension))
            {
                EncryptionExtensions.Remove(extension);
            }
        }

        /// <summary>
        /// Choose the log directory.
        /// </summary>
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

    public class ConfigurationData
    {
        public string LogFormat { get; set; } = "JSON";
        public string BusinessSoftware { get; set; } = "Calculator";
        public string LogDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
        public List<string> EncryptionExtensions { get; set; } = new();
        public List<string> PriorityExtensions { get; set; } = new();
    }
}
