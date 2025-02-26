using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EasySave.Remote.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            // Charger la configuration depuis appsettings.GUI.json (optionnel)
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.GUI.json", optional: true, reloadOnChange: true)
                .Build();

            // Démarrer le serveur
            Server.Start(configuration);
        }
    }
}
