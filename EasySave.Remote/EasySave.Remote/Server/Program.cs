using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EasySave.Remote.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.GUI.json", optional: true, reloadOnChange: true)
                .Build();

            Server.Start(configuration);
        }
    }
}
