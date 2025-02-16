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

namespace EasySave.GUI.ViewModels
{
    public class ConfigurationViewModel : ReactiveObject
    {
        private string _logFormat = "XML";
        private string _businessSoftware = "Calculator";
        private string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
        private ObservableCollection<string> _encryptionExtensions = new();

        // Instance du gestionnaire de langue
        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        public string LogFormat
        {
            get => _logFormat;
            set => this.RaiseAndSetIfChanged(ref _logFormat, value);
        }

        public string LogDirectory
        {
            get => _logDirectory;
            set => this.RaiseAndSetIfChanged(ref _logDirectory, value);
        }

        public string BusinessSoftware
        {
            get => _businessSoftware;
            set => this.RaiseAndSetIfChanged(ref _businessSoftware, value);
        }

        public ObservableCollection<string> EncryptionExtensions
        {
            get => _encryptionExtensions;
            set => this.RaiseAndSetIfChanged(ref _encryptionExtensions, value);
        }

        public List<string> LogFormatOptions { get; } = new() { "JSON", "XML" };

        public ReactiveCommand<string, Unit> AddExtensionCommand { get; }
        public ReactiveCommand<string, Unit> RemoveExtensionCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Window, Unit> ChooseLogDirectoryCommand { get; }
        public ReactiveCommand<Window, Unit> CloseCommand { get; }

        public ConfigurationViewModel()
        {
            LoadSettings();
            AddExtensionCommand = ReactiveCommand.Create<string>(AddExtension);
            RemoveExtensionCommand = ReactiveCommand.Create<string>(RemoveExtension);
            SaveCommand = ReactiveCommand.Create(SaveSettings);
            ChooseLogDirectoryCommand = ReactiveCommand.CreateFromTask<Window>(ChooseLogDirectory);
            
            // Correction de CloseCommand avec vérification de null
            CloseCommand = ReactiveCommand.Create<Window>(window =>
            {
                if (window != null)
                {
                    window.Close();
                }
            });
        }

        public void LoadSettings()
        {
            try
            {
                string configPath = "appsettings.GUI.json";
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ConfigurationData>(json);

                    LogFormat = config?.LogFormat ?? "JSON";
                    BusinessSoftware = config?.BusinessSoftware ?? "Calculator";
                    LogDirectory = config?.LogDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
                    EncryptionExtensions = new ObservableCollection<string>(config?.EncryptionExtensions ?? new List<string> { ".txt", ".docx" });

                    Debug.WriteLine($"📂 {LanguageHelperInstance.LogFormatLabel} : {LogDirectory}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ {LanguageHelperInstance.ErrorLoadingFiles} {ex.Message}");
            }
        }

        public void SaveSettings()
        {
            try
            {
                var config = new ConfigurationData
                {
                    LogFormat = LogFormat,
                    BusinessSoftware = BusinessSoftware,
                    LogDirectory = LogDirectory,
                    EncryptionExtensions = EncryptionExtensions.ToList()
                };

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("appsettings.GUI.json", json);

                Debug.WriteLine($"✅ {LanguageHelperInstance.ButtonSave} - {LogDirectory}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ {LanguageHelperInstance.ErrorLoadingFiles} {ex.Message}");
            }
        }

        public void AddExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension) && !EncryptionExtensions.Contains(extension))
            {
                EncryptionExtensions.Add(extension);
            }
        }

        public void RemoveExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension) && EncryptionExtensions.Contains(extension))
            {
                EncryptionExtensions.Remove(extension);
            }
        }

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
    }
}
