using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using EasySaveLogs; // Assurez-vous que Logger et ConfigurationReloader sont dans ce namespace

namespace EasySave.GUI
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // Charger la configuration spécifique à l'application graphique depuis appsettings.GUI.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.GUI.json", optional: false, reloadOnChange: true)
                .Build();

            // Récupérer les valeurs spécifiques à la GUI
            string logDirectory = configuration["LogDirectory"] ?? "/Users/tpellizzari/Logs";
            string logFormat = configuration["LogFormat"] ?? "JSON";

            // Créer l'instance du Logger avec ces valeurs
            Logger logger = Logger.GetInstance(logDirectory, logFormat);

            // Créer le reloader pour surveiller les changements de configuration
            var reloader = new ConfigurationReloader(logger);

            // Lancer l'application Avalonia
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
