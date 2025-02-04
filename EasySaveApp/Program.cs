using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using EasySave.Core.Facade;
using EasySave.Core.Models;
using EasySave.Core.Utils;
using EasySave.Core.Observers;
using EasySave.Core.Repositories;

namespace EasySaveApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Charger la configuration depuis appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();

            // Lire la config pour la langue avec une valeur par défaut "en"
            string language = configuration["Language"];

            // Vérification pour éviter de passer un `null` à `SetLanguage()`
            if (string.IsNullOrWhiteSpace(language) || (language != "en" && language != "fr"))
            {
                do
                {
                    Console.WriteLine("Choose language (en/fr): ");
                    language = Console.ReadLine()?.Trim().ToLower();
                } while (language != "en" && language != "fr");
            }

            // Appliquer la langue
            LanguageHelper.Instance.SetLanguage(language);
            Console.WriteLine($"Language selected: {language}");

            // Définir le répertoire de logs selon la configuration et l'OS
            string logDirectory = configuration["Logging:LogDirectory"];
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                logDirectory = GetDefaultLogDirectory();
            }

            // Créer le dossier de logs s'il n'existe pas
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Créer le repository pour la persistance (JSON)
            string repositoryPath = "backup_jobs.json";
            IBackupJobRepository jobRepository = new JsonBackupJobRepository(repositoryPath);

            // Créer un observer pour l'état (fichier state.json)
            string stateFilePath = "state.json";
            var fileStateObserver = new FileStateObserver(stateFilePath);

            // Instancier la Façade avec les dépendances
            var facade = new EasySaveFacade(jobRepository, logDirectory, fileStateObserver);

            // Vérifier si des arguments en ligne de commande sont passés
            if (args.Length > 0)
            {
                ExecuteBackupFromArgs(facade, args);
                return;
            }

            // Boucle principale de l’application (menu)
            while (true)
            {
                Console.WriteLine("\n--- " + LanguageHelper.Instance.GetMessage("MenuTitle") + " ---");
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
                        break;
                    case "3":
                        facade.ListJobs();
                        break;
                    case "4":
                        Console.Write(LanguageHelper.Instance.GetMessage("EnterIndexRemove"));
                        if (int.TryParse(Console.ReadLine(), out int removeIndex))
                        {
                            facade.RemoveJob(removeIndex - 1);
                        }
                        else
                        {
                            Console.WriteLine(LanguageHelper.Instance.GetMessage("InvalidIndex"));
                        }
                        break;
                    case "5":
                        Console.Write(LanguageHelper.Instance.GetMessage("EnterIndexUpdate"));
                        if (int.TryParse(Console.ReadLine(), out int updateIndex))
                        {
                            UpdateJob(facade, updateIndex - 1);
                        }
                        else
                        {
                            Console.WriteLine(LanguageHelper.Instance.GetMessage("InvalidIndex"));
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

        private static void ExecuteBackupFromArgs(EasySaveFacade facade, string[] args)
        {
            string param = args[0];
            List<int> jobIndices = new List<int>();

            var segments = param.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                if (segment.Contains("-"))
                {
                    var rangeParts = segment.Split('-');
                    if (rangeParts.Length == 2 &&
                        int.TryParse(rangeParts[0], out int start) &&
                        int.TryParse(rangeParts[1], out int end))
                    {
                        for (int i = start; i <= end; i++)
                        {
                            jobIndices.Add(i - 1);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠ Erreur : Format de plage invalide dans '{segment}'.");
                    }
                }
                else if (int.TryParse(segment, out int singleIndex))
                {
                    jobIndices.Add(singleIndex - 1);
                }
                else
                {
                    Console.WriteLine($"⚠ Erreur : Format invalide pour '{segment}'.");
                }
            }

            jobIndices = jobIndices.Distinct().OrderBy(x => x).ToList();

            foreach (var index in jobIndices)
            {
                if (index < facade.GetJobCount())
                {
                    facade.ExecuteJobByIndex(index);
                }
                else
                {
                    Console.WriteLine($"⚠ Erreur : Index {index + 1} hors de portée.");
                }
            }
        }

        private static BackupJob? CreateJobFromConsole()
        {
            Console.WriteLine("\n" + LanguageHelper.Instance.GetMessage("AddJobTitle"));

            Console.Write(LanguageHelper.Instance.GetMessage("EnterJobName"));
            var name = Console.ReadLine()?.Trim();

            Console.Write(LanguageHelper.Instance.GetMessage("EnterSourceDir"));
            var sourceDirectory = Console.ReadLine()?.Trim();

            Console.Write(LanguageHelper.Instance.GetMessage("EnterTargetDir"));
            var targetDirectory = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(sourceDirectory) || string.IsNullOrEmpty(targetDirectory))
            {
                Console.WriteLine(LanguageHelper.Instance.GetMessage("ErrorFieldsRequired"));
                return null;
            }

            Console.WriteLine(LanguageHelper.Instance.GetMessage("SelectBackupType"));
            Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionComplete"));
            Console.WriteLine(LanguageHelper.Instance.GetMessage("OptionDifferential"));
            Console.Write("> ");
            var backupTypeInput = Console.ReadLine();
            BackupType backupType = backupTypeInput == "1" ? BackupType.Complete : BackupType.Differential;

            return new BackupJob(name, sourceDirectory, targetDirectory, backupType);
        }

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
            BackupType? newType = typeInput == "1" ? BackupType.Complete : typeInput == "2" ? BackupType.Differential : null;

            facade.UpdateJob(index, newName, newSource, newTarget, newType);
        }

        private static string GetDefaultLogDirectory()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\Logs" : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Logs";
        }
    }
}
