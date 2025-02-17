using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySaveLogs
{
    /// <summary>
    /// Surveille les modifications du fichier de configuration (appsettings.json)
    /// et reconfigure dynamiquement le Logger.
    /// </summary>
    public class ConfigurationReloader
    {
        private readonly string _configPath = "appsettings.GUI.json";
        private readonly Logger _logger;

        public ConfigurationReloader(Logger logger)
        {
            _logger = logger;
            // Récupérer le répertoire contenant le fichier de configuration.
            string? directory = Path.GetDirectoryName(_configPath);

            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Directory.GetCurrentDirectory();
            }
            string fileName = Path.GetFileName(_configPath);

            var watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            watcher.Changed += OnConfigChanged;
        }

        private void OnConfigChanged(object sender, FileSystemEventArgs e)
        {
            // Attendre un court instant pour éviter des conflits de lecture
            Task.Delay(500).Wait();

            try
            {
                string json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<ConfigurationData>(json);
                if (config != null)
                {
                    // Reconfigure le Logger avec la nouvelle valeur
                    _logger.Reconfigure(config.LogFormat);
                    Console.WriteLine($"Configuration rechargée : LogFormat = {config.LogFormat}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du rechargement de la configuration : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Représente les données de configuration pour le logger.
    /// </summary>
    public class ConfigurationData
    {
        public string LogFormat { get; set; } = "JSON";
    }
}
