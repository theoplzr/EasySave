using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace EasySave.Core
{
    /// <summary>
    /// Provides static access to certain configuration values such as log format,
    /// file encryption extensions, business software name, and encryption key.
    /// </summary>
    public static class ConfigurationProvider
    {
        private static readonly IConfiguration _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.GUI.json", optional: true, reloadOnChange: true)
            .Build();

        /// <summary>
        /// Gets the log format (e.g., "JSON", "XML").
        /// </summary>
        public static string LogFormat => _configuration["LogFormat"];

        /// <summary>
        /// Gets an array of file extensions that should be encrypted.
        /// </summary>
        public static string[] EncryptionExtensions =>
            (_configuration["EncryptionExtensions"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Gets the name of the business software to detect.
        /// </summary>
        public static string BusinessSoftware => _configuration["BusinessSoftware"];

        /// <summary>
        /// Encryption key used by CryptoSoft. Defaults to "DefaultKey123" if not set.
        /// </summary>
        public static string EncryptionKey { get; set; } = "DefaultKey123";
    }
}
