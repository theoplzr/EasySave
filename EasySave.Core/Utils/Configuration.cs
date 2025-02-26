using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace EasySave.Core.Utils
{
    /// <summary>
    /// Provides methods to load and access application configuration values from a JSON file.
    /// </summary>
    public static class Configuration
    {
        private static IConfigurationRoot _config;

        /// <summary>
        /// Static constructor to initialize configuration loading.
        /// </summary>
        static Configuration()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Loads the configuration from the appsettings.json file if present,
        /// otherwise sets default values.
        /// </summary>
        private static void LoadConfiguration()
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(basePath, "appsettings.json");

                if (!File.Exists(configPath))
                {
                    Console.WriteLine($"⚠️ Warning: Configuration file '{configPath}' not found. Using default values.");
                    SetDefaultConfig();
                    return;
                }

                var builder = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                _config = builder.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading configuration: {ex.Message}");
                SetDefaultConfig();
            }
        }

        /// <summary>
        /// Applies default configuration if the file is missing or loading fails.
        /// </summary>
        private static void SetDefaultConfig()
        {
            var defaultConfig = new Dictionary<string, string>
            {
                { "Logging:LogFormat", "JSON" },
                { "Logging:BusinessSoftware", "Calculator" },
                { "Logging:LogDirectory", "/Users/tpellizzari/Logs" },
                { "Logging:EncryptionKey", "DefaultKey123" }
            };

            var builder = new ConfigurationBuilder().AddInMemoryCollection(defaultConfig);
            _config = builder.Build();
        }

        /// <summary>
        /// Retrieves the format for logs (e.g. JSON, XML).
        /// </summary>
        /// <returns>A string specifying the log format.</returns>
        public static string GetLogFormat()
        {
            return _config?["Logging:LogFormat"] ?? "JSON";
        }

        /// <summary>
        /// Retrieves the list of file extensions that should be encrypted.
        /// </summary>
        /// <returns>A list of file extensions (e.g. ".txt", ".docx").</returns>
        public static List<string> GetCryptoExtensions()
        {
            var extensions = _config?.GetSection("Logging:EncryptionExtensions").Get<List<string>>();
            return extensions ?? new List<string> { ".txt", ".docx" };
        }

        /// <summary>
        /// Retrieves the name of the business software to monitor.
        /// </summary>
        /// <returns>The name of the process to detect, or "Calculator" by default.</returns>
        public static string GetBusinessSoftware()
        {
            return _config?["Logging:BusinessSoftware"] ?? "Calculator";
        }

        /// <summary>
        /// Retrieves the directory in which logs are stored.
        /// </summary>
        /// <returns>The path to the logging directory.</returns>
        public static string GetLogDirectory()
        {
            string logDir = _config?["Logging:LogDirectory"]
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
            Console.WriteLine($"🔍 GetLogDirectory() → {logDir}");
            return logDir;
        }

        /// <summary>
        /// Retrieves the encryption key used by CryptoSoft.
        /// </summary>
        /// <returns>A string containing the encryption key. "DefaultKey123" if not set.</returns>
        public static string GetEncryptionKey()
        {
            return _config?["Logging:EncryptionKey"] ?? "DefaultKey123";
        }
    }
}
