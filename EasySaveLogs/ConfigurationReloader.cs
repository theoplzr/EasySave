using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySaveLogs
{
    /// <summary>
    /// Monitors the configuration file (appsettings.GUI.json) and dynamically
    /// reconfigures the <see cref="Logger"/> when changes are detected.
    /// </summary>
    public class ConfigurationReloader
    {
        private readonly string _configPath = "appsettings.GUI.json";
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationReloader"/> class.
        /// Sets up a file watcher to detect changes in the configuration file.
        /// </summary>
        /// <param name="logger">The <see cref="Logger"/> instance to be reconfigured.</param>
        public ConfigurationReloader(Logger logger)
        {
            _logger = logger;

            // Retrieve the directory containing the configuration file
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

        /// <summary>
        /// Triggered when the configuration file changes. Delays briefly to avoid reading conflicts,
        /// then updates the logger's format if possible.
        /// </summary>
        private void OnConfigChanged(object sender, FileSystemEventArgs e)
        {
            // Wait briefly to avoid file reading conflicts
            Task.Delay(500).Wait();

            try
            {
                string json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<ConfigurationData>(json);
                if (config != null)
                {
                    // Dynamically reconfigure the logger
                    _logger.Reconfigure(config.LogFormat);
                    Console.WriteLine($"Configuration reloaded: LogFormat = {config.LogFormat}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reloading configuration: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Represents logger configuration data (e.g., log format).
    /// </summary>
    public class ConfigurationData
    {
        /// <summary>
        /// Specifies the desired log format (e.g., "JSON", "XML").
        /// </summary>
        public string LogFormat { get; set; } = "JSON";
    }
}
