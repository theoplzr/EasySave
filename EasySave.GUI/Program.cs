using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using CryptoSoftLib;
using EasySaveLogs;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace EasySave.GUI
{
    /// <summary>
    /// Entry point for the EasySave GUI application using Avalonia.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main method that initializes the application and starts the Avalonia event loop.
        /// </summary>
        /// <param name="args">Optional command-line arguments (not typically used here).</param>
        public static void Main(string[] args)
        {
            // Load GUI-specific configuration from appsettings.GUI.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.GUI.json", optional: false, reloadOnChange: true)
                .Build();

            // Retrieve GUI-specific values
            string logDirectory = configuration["LogDirectory"] ?? string.Empty;
            string logFormat = configuration["LogFormat"] ?? "JSON";

            // Create a logger instance with these values
            Logger logger = Logger.GetInstance(logDirectory, logFormat);

            // Create a reloader to monitor configuration changes
            var reloader = new ConfigurationReloader(logger);

            // Start the Avalonia application
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        /// <summary>
        /// Configures the Avalonia application builder, enabling platform detection and ReactiveUI.
        /// </summary>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
