using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace EasySave.Core.Utils
{
    public static class Configuration
    {
        private static IConfigurationRoot _config;

        static Configuration()
        {
            LoadConfiguration();
        }

        private static void LoadConfiguration()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory; 
            string configPath = Path.Combine(basePath, "appsettings.json"); 

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Le fichier de configuration '{configPath}' est introuvable.");
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _config = builder.Build();
        }

        public static string GetLogFormat()
        {
            return _config["Logging:LogFormat"] ?? "JSON"; // Défaut JSON
        }

        public static List<string> GetCryptoExtensions()
        {
            return _config.GetSection("Logging:EncryptionExtensions").Get<List<string>>() ?? new List<string> { ".txt", ".docx" };
        }

        public static string GetBusinessSoftware()
        {
            return _config["Logging:BusinessSoftware"] ?? "Calculator";
        }

        public static string GetLogDirectory()
        {
            return _config["Logging:LogDirectory"] ?? "/Users/tpellizzari/Logs";
        }
    }
}
