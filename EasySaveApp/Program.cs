using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using EasySave.Core.Facade;
using EasySave.Core.Models;
using EasySave.Core.Utils;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasySaveApp
{
    /// <summary>
    /// Main entry point for the EasySave application. Manages user interactions,
    /// configuration, and initialization of core components.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command-line arguments specifying backup job indices or ranges (optional).</param>
        private static void Main(string[] args)
        {
            // Load configuration from appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();

            // Read the language setting from configuration. Default is "en" if not provided.
            string? language = configuration["Language"];

            // Validate and prompt for language if necessary
            if (string.IsNullOrWhiteSpace(language) || (language != "en" && language != "fr"))
            {
                do
                {
                    Console.WriteLine("Choose language (en/fr): ");
                    language = Console.ReadLine()?.Trim().ToLower();
                } 
                while (language != "en" && language != "fr");
            }

            // Apply selected language
            LanguageHelper.Instance.SetLanguage(language);
            Console.WriteLine($"Language selected: {language}");

            // Retrieve the log directory from configuration or use a default path
            string logDirectory = configuration["Logging:LogDirectory"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                logDirectory = GetDefaultLogDirectory();
            }

            // Create the log directory if it doesn't exist
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Create the repository for persistence (JSON-based)
            string repositoryPath = "backup_jobs.json";
            IBackupJobRepository jobRepository = new JsonBackupJobRepository(repositoryPath);

            // Create a file state observer (writes state to state.json)
            string stateFilePath = "state.json";
            var fileStateObserver = new FileStateObserver(stateFilePath);

            // Instantiate the facade with required dependencies
            var facade = new EasySaveFacade(jobRepository, logDirectory, fileStateObserver, configuration);

            // If command-line arguments were provided, handle backup jobs from these arguments and exit
            if (args.Length > 0)
            {
                ExecuteBackupFromArgs(facade, args);
                return;
            }

            // Main application loop (menu)
            while (true)
            {
                Console.WriteLine($"\n--- {LanguageHelper.Instance.GetMessage("MenuTitle")} ---");
                Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionAddJob"));
                Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionExecuteAll"));
                Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionListJobs"));
                Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionRemoveJob"));
                Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionUpdateJob"));
                Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionExit"));
                Console.Write("> ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        var job = CreateJobFromConsole();
                        if (job != null)
                        {
                            facade.AddJob(job);
                        }
                        break;

                    case "2":
                        facade.ExecuteAllJobs();
                        Console.WriteLine("🚀  All jobs executed successfully.");
                        break;

                    case "3":
                        facade.ListJobs();
                        Console.WriteLine("✅  Listing of jobs completed.");
                        break;

                    case "4":
                        Console.Write(LanguageHelper.Instance.GetMessage("EnterIndexRemove"));
                        if (!int.TryParse(Console.ReadLine(), out int removeIndex) 
                            || removeIndex < 1 
                            || removeIndex > facade.GetJobCount())
                        {
                            Console.WriteLine("❌ Invalid job index. Please enter a valid number.");
                        }
                        else
                        {
                            facade.RemoveJob(removeIndex - 1);
                            Console.WriteLine($"✅ Job {removeIndex} removed successfully.");
                        }
                        break;

                    case "5":
                        Console.Write(LanguageHelper.Instance.GetMessage("EnterIndexUpdate"));
                        if (!int.TryParse(Console.ReadLine(), out int updateIndex) 
                            || updateIndex < 1 
                            || updateIndex > facade.GetJobCount())
                        {
                            Console.WriteLine("❌ Invalid job index. Please enter a valid number.");
                        }
                        else
                        {
                            UpdateJob(facade, updateIndex - 1);
                            Console.WriteLine($"✅ Job {updateIndex} updated successfully.");
                        }
                        break;

                    case "6":
                        Console.WriteLine(LanguageHelper.Instance.GetMessage("Goodbye"));
                        return;

                    default:
                        Console.WriteLine(LanguageHelper.Instance.GetMessage("InvalidChoice"));
                        break;
                }
            }
        }

        /// <summary>
        /// Executes backup jobs based on command-line arguments, which can be single indices or ranges.
        /// </summary>
        /// <param name="facade">The <see cref="EasySaveFacade"/> managing the backup operations.</param>
        /// <param name="args">Command-line arguments specifying job indices/ranges, e.g. "1;3-5;7".</param>
        private static void ExecuteBackupFromArgs(EasySaveFacade facade, string[] args)
        {
            string param = args[0];
            var jobIndices = new List<int>();

            var segments = param.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                if (segment.Contains("-"))
                {
                    var rangeParts = segment.Split('-');
                    if (rangeParts.Length == 2 
                        && int.TryParse(rangeParts[0], out int start) 
                        && int.TryParse(rangeParts[1], out int end))
                    {
                        for (int i = start; i <= end; i++)
                        {
                            jobIndices.Add(i - 1);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠ Error: Invalid range format in '{segment}'.");
                    }
                }
                else if (int.TryParse(segment, out int singleIndex))
                {
                    jobIndices.Add(singleIndex - 1);
                }
                else
                {
                    Console.WriteLine($"⚠ Error: Invalid format '{segment}'.");
                }
            }

            // Remove duplicates and sort indices
            jobIndices = jobIndices.Distinct().OrderBy(x => x).ToList();

            // Execute each valid job index
            foreach (var index in jobIndices)
            {
                if (index < facade.GetJobCount())
                {
                    facade.ExecuteJobByIndex(index);
                }
                else
                {
                    Console.WriteLine($"⚠ Error: Index {index + 1} is out of range.");
                }
            }
        }

        /// <summary>
        /// Prompts the user to create a new <see cref="BackupJob"/> by entering details in the console.
        /// </summary>
        /// <returns>A new <see cref="BackupJob"/> if all fields are valid; otherwise <c>null</c>.</returns>
        private static BackupJob? CreateJobFromConsole()
        {
            Console.WriteLine($"\n{LanguageHelper.Instance.GetMessage("AddJobTitle")}");

            Console.Write(LanguageHelper.Instance.GetMessage("EnterJobName"));
            string? name = Console.ReadLine()?.Trim();

            Console.Write(LanguageHelper.Instance.GetMessage("EnterSourceDir"));
            string? sourceDirectory = Console.ReadLine()?.Trim();

            Console.Write(LanguageHelper.Instance.GetMessage("EnterTargetDir"));
            string? targetDirectory = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(name) 
                || string.IsNullOrEmpty(sourceDirectory) 
                || string.IsNullOrEmpty(targetDirectory))
            {
                Console.WriteLine(LanguageHelper.Instance.GetMessage("ErrorFieldsRequired"));
                return null;
            }

            Console.WriteLine(LanguageHelper.Instance.GetMessage("SelectBackupType"));
            Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionComplete"));
            Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionDifferential"));
            Console.Write("> ");
            string? backupTypeInput = Console.ReadLine();

            BackupType backupType = (backupTypeInput == "1") 
                ? BackupType.Complete 
                : BackupType.Differential;

            return new BackupJob(name, sourceDirectory, targetDirectory, backupType);
        }

        /// <summary>
        /// Prompts the user to update the properties of an existing backup job.
        /// </summary>
        /// <param name="facade">The <see cref="EasySaveFacade"/> instance managing backup jobs.</param>
        /// <param name="index">Zero-based index of the job to update.</param>
        private static void UpdateJob(EasySaveFacade facade, int index)
        {
            Console.Write(LanguageHelper.Instance.GetMessage("EnterNewName"));
            string? newName = Console.ReadLine();

            Console.Write(LanguageHelper.Instance.GetMessage("EnterNewSourceDir"));
            string? newSource = Console.ReadLine();

            Console.Write(LanguageHelper.Instance.GetMessage("EnterNewTargetDir"));
            string? newTarget = Console.ReadLine();

            Console.Write(LanguageHelper.Instance.GetMessage("EnterNewBackupType"));
            string? typeInput = Console.ReadLine();

            BackupType? newType = typeInput == "1" 
                ? BackupType.Complete 
                : typeInput == "2" 
                    ? BackupType.Differential 
                    : null;

            facade.UpdateJob(index, newName, newSource, newTarget, newType);
        }

        /// <summary>
        /// Gets the default directory for log files based on the current operating system.
        /// </summary>
        /// <returns>A string representing the default log directory path.</returns>
        private static string GetDefaultLogDirectory()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? @"C:\Logs" 
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs");
        }
    }
}
