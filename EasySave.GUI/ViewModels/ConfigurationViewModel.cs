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
        private string _cryptoTestResult = "Non testé";
        private ObservableCollection<string> _encryptionExtensions = new();

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

        public string CryptoTestResult
        {
            get => _cryptoTestResult;
            set => this.RaiseAndSetIfChanged(ref _cryptoTestResult, value);
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
        public ReactiveCommand<Unit, Unit> TestCryptoSoftCommand { get; }
        public ReactiveCommand<Window, Unit> ChooseLogDirectoryCommand { get; }

        public ConfigurationViewModel()
        {
            LoadSettings();
            AddExtensionCommand = ReactiveCommand.Create<string>(AddExtension);
            RemoveExtensionCommand = ReactiveCommand.Create<string>(RemoveExtension);
            SaveCommand = ReactiveCommand.Create(SaveSettings);
            TestCryptoSoftCommand = ReactiveCommand.CreateFromTask(TestCryptoSoft);
            ChooseLogDirectoryCommand = ReactiveCommand.CreateFromTask<Window>(ChooseLogDirectory);
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
                    LogDirectory = !string.IsNullOrWhiteSpace(config?.LogDirectory)
                        ? config.LogDirectory
                        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
                    EncryptionExtensions = new ObservableCollection<string>(
                        config?.EncryptionExtensions ?? new List<string> { ".txt", ".docx" }
                    );

                    Console.WriteLine($"📂 Logs seront enregistrés dans : {LogDirectory}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors du chargement de la configuration : {ex.Message}");
            }
        }

        public void SaveSettings()
        {
            try
            {
                Console.WriteLine("✅ SaveSettings() appelé");

                var config = new ConfigurationData
                {
                    LogFormat = LogFormat,
                    BusinessSoftware = BusinessSoftware,
                    LogDirectory = LogDirectory,
                    EncryptionExtensions = EncryptionExtensions.ToList()
                };

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("appsettings.json", json);

                Console.WriteLine($"✅ Configuration enregistrée avec succès. 📂 Logs seront stockés dans : {LogDirectory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de l'enregistrement de la configuration : {ex.Message}");
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

        /// <summary>
        /// Permet de sélectionner un dossier pour enregistrer les logs.
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

        /// <summary>
        /// Teste l'exécution de CryptoSoft sur un fichier temporaire.
        /// </summary>
        private async Task TestCryptoSoft()
        {
            // Définir le chemin correct de CryptoSoft
            string cryptoPath = "/Users/tpellizzari/Desktop/CESI-A3/Génie logiciel/Projet/EasySave/CryptoSoft/out/CryptoSoft";

            if (!File.Exists(cryptoPath))
            {
                CryptoTestResult = "❌ CryptoSoft introuvable ! Vérifie son emplacement.";
                return;
            }

            string testFile = "test_crypto.txt";
            string key = "mysecurekey";

            try
            {
                // Créer un fichier test
                await File.WriteAllTextAsync(testFile, "Test de cryptage EasySave");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = cryptoPath,
                        Arguments = $"{testFile} {key}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                CryptoTestResult = process.ExitCode > 0
                    ? $"✅ Chiffrement réussi en {process.ExitCode} ms !"
                    : "❌ Erreur de cryptage !";
            }
            catch (Exception ex)
            {
                CryptoTestResult = $"❌ Erreur lors du test de CryptoSoft : {ex.Message}";
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
