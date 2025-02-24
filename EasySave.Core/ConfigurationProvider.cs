using Microsoft.Extensions.Configuration;
using System.IO;

namespace EasySave.Core
{
    public static class ConfigurationProvider
    {
        private static readonly IConfiguration _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.GUI.json", optional: true, reloadOnChange: true)
            .Build();

        public static string LogFormat => _configuration["LogFormat"] ;
        public static string[] EncryptionExtensions => (_configuration["EncryptionExtensions"] ?? "").Split(',', System.StringSplitOptions.RemoveEmptyEntries);
        public static string BusinessSoftware => _configuration["BusinessSoftware"];
        public static string EncryptionKey { get; set; } = "DefaultKey123"; 
    }
}
