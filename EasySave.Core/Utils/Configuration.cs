using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Charge la configuration à partir du fichier JSON.
        /// </summary>
        private static void LoadConfiguration()
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(basePath, "appsettings.json");

                if (!File.Exists(configPath))
                {
                    Console.WriteLine($"⚠️ Attention : Le fichier de configuration '{configPath}' est introuvable. Utilisation des valeurs par défaut.");
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
                Console.WriteLine($"❌ Erreur lors du chargement de la configuration : {ex.Message}");
                SetDefaultConfig();
            }
        }

        /// <summary>
        /// Définit une configuration par défaut en cas d'erreur ou de fichier manquant.
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
        /// Récupère le format de log (JSON ou XML).
        /// </summary>
        public static string GetLogFormat()
        {
            return _config?["Logging:LogFormat"] ?? "JSON";
        }

        /// <summary>
        /// Récupère la liste des extensions de fichiers à crypter.
        /// </summary>
        public static List<string> GetCryptoExtensions()
        {
            var extensions = _config?.GetSection("Logging:EncryptionExtensions").Get<List<string>>();
            return extensions ?? new List<string> { ".txt", ".docx" };
        }

        /// <summary>
        /// Récupère le nom du logiciel métier à surveiller.
        /// </summary>
        public static string GetBusinessSoftware()
        {
            return _config?["Logging:BusinessSoftware"] ?? "Calculator";
        }

        /// <summary>
        /// Récupère le répertoire où enregistrer les logs.
        /// </summary>
        public static string GetLogDirectory()
        {
            string logDir = _config?["Logging:LogDirectory"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
            Console.WriteLine($"🔍 GetLogDirectory() → {logDir}");
            return logDir;
        }

        /// <summary>
        /// Récupère la clé de cryptage utilisée par CryptoSoft.
        /// </summary>
        public static string GetEncryptionKey()
        {
            return _config?["Logging:EncryptionKey"] ?? "DefaultKey123"; // Clé par défaut
        }
    }
}
