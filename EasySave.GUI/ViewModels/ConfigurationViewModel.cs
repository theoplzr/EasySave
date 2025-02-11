using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using ReactiveUI;
using System.Reactive;
using System.Linq;
using EasySave.GUI.Helpers;

namespace EasySave.GUI.ViewModels
{
    public class ConfigurationViewModel : ReactiveObject
    {
        private string _logFormat = "JSON";
        private string _businessSoftware = "Calculator";
        private ObservableCollection<string> _encryptionExtensions = new();

        public string LogFormat
        {
            get => _logFormat;
            set => this.RaiseAndSetIfChanged(ref _logFormat, value);
        }

        public LanguageHelper LanguageHelperInstance => LanguageHelper.Instance;

        public List<string> LogFormatOptions { get; } = new() { "JSON", "XML" };

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

        public ReactiveCommand<string, Unit> AddExtensionCommand { get; }
        public ReactiveCommand<string, Unit> RemoveExtensionCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public ConfigurationViewModel()
        {
            LoadSettings();
            AddExtensionCommand = ReactiveCommand.Create<string>(AddExtension);
            RemoveExtensionCommand = ReactiveCommand.Create<string>(RemoveExtension);
            SaveCommand = ReactiveCommand.Create(SaveSettings);
        }

        public void LoadSettings()
        {
            try
            {
                string configPath = "appsettings.json";
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ConfigurationData>(json);

                    LogFormat = config?.LogFormat ?? "JSON";
                    BusinessSoftware = config?.BusinessSoftware ?? "Calculator";
                    EncryptionExtensions = new ObservableCollection<string>(
                        config?.EncryptionExtensions ?? new List<string> { ".txt", ".docx" }
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
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
                    EncryptionExtensions = EncryptionExtensions.ToList() // Utilisé uniquement ici pour la sérialisation
                };

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("appsettings.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
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
    }

    public class ConfigurationData
    {
        public string LogFormat { get; set; } = "JSON";
        public string BusinessSoftware { get; set; } = "Calculator";
        public List<string> EncryptionExtensions { get; set; } = new();
    }
}
