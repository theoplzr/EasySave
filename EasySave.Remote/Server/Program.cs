using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EasySave.Remote.Server
{
    // Main entry point for the remote server application.
    class Program
    {
        static void Main(string[] args)
        {
            // Build configuration from appsettings.GUI.json file.
            IConfiguration configuration = new ConfigurationBuilder()
                // Set the base path to the current directory.
                .SetBasePath(Directory.GetCurrentDirectory())
                // Add the JSON configuration file; optional and reloadable on changes.
                .AddJsonFile("appsettings.GUI.json", optional: true, reloadOnChange: true)
                // Build the configuration.
                .Build();

            // Start the server with the built configuration.
            Server.Start(configuration);
        }
    }
}
