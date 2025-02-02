using Microsoft.Extensions.Configuration;
using EasySaveApp.Facade;
using EasySaveApp.Models;
using EasySaveApp.Utils;
using EasySaveApp.Observers;
using EasySaveApp.Repositories; 
namespace EasySaveApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1) Charger la configuration depuis appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            // 2) Lire la config pour la langue et le répertoire de logs
            string language = configuration["Language"] ?? "en";
            string logDirectory = configuration["Logging:LogDirectory"] ?? "Logs";

            // 3) Créer le repository pour la persistance (JSON)
            string repositoryPath = "backup_jobs.json";
            IBackupJobRepository jobRepository = new JsonBackupJobRepository(repositoryPath);

            // 4) Créer un observer pour l'état (fichier state.json)
            string stateFilePath = "state.json";
            var fileStateObserver = new FileStateObserver(stateFilePath);

            // 5) Instancier la Façade en lui injectant le repository, le logDirectory, et l’observer
            var facade = new EasySaveFacade(jobRepository, logDirectory, fileStateObserver);

            // 6) Vérifier si des arguments en ligne de commande sont passés
            if (args.Length > 0)
            {
                ExecuteBackupFromArgs(facade, args); 
                return;
            }

            // 7) Demander à l’utilisateur de choisir la langue
            Console.Write("Choose language (en/fr): ");
            var userInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(userInput) && (userInput == "fr" || userInput == "en"))
            {
                language = userInput;
            }

            // 8) Boucle principale de l’application (menu)
            while (true)
            {
                Console.WriteLine("\n--- " + LanguageHelper.GetMessage("MenuTitle", language) + " ---");
                Console.WriteLine("1) Add a backup job");
                Console.WriteLine("2) Execute all jobs");
                Console.WriteLine("3) List all jobs");
                Console.WriteLine("4) Remove a job");
                Console.WriteLine("5) Update a job");
                Console.WriteLine("6) Exit");
                Console.Write("> ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        {
                            // Créer un nouveau job via la console
                            var job = CreateJobFromConsole(language);
                            if (job != null)
                            {
                                // Appel direct à la façade
                                facade.AddJob(job);
                            }
                            break;
                        }
                    case "2":
                        {
                            // Exécuter tous les jobs
                            facade.ExecuteAllJobs();
                            break;
                        }
                    case "3":
                        {
                            // Lister les jobs
                            facade.ListJobs();
                            break;
                        }
                    case "4":
                        {
                            // Supprimer un job
                            Console.Write("Enter the index of the job to remove: ");
                            if (int.TryParse(Console.ReadLine(), out int removeIndex))
                            {
                                removeIndex -= 1; // Conversion en zero-based
                                facade.RemoveJob(removeIndex);
                            }
                            else
                            {
                                Console.WriteLine("Invalid index.");
                            }
                            break;
                        }
                    case "5":
                        {
                            // Mettre à jour un job
                            Console.Write("Enter the index of the job to update: ");
                            if (int.TryParse(Console.ReadLine(), out int updateIndex))
                            {
                                updateIndex -= 1; // zéro-based

                                // Paramètres de mise à jour
                                Console.WriteLine("Enter new name (leave blank to keep existing): ");
                                string? newName = Console.ReadLine();

                                Console.WriteLine("Enter new source dir (leave blank to keep existing): ");
                                string? newSource = Console.ReadLine();

                                Console.WriteLine("Enter new target dir (leave blank to keep existing): ");
                                string? newTarget = Console.ReadLine();

                                Console.WriteLine("Enter new backup type (1: Complete, 2: Differential), leave blank to keep existing: ");
                                string? typeInput = Console.ReadLine();
                                BackupType? newType = null;
                                if (typeInput == "1") newType = BackupType.Complete;
                                else if (typeInput == "2") newType = BackupType.Differential;

                                facade.UpdateJob(updateIndex, newName, newSource, newTarget, newType);
                            }
                            else
                            {
                                Console.WriteLine("Invalid index.");
                            }
                            break;
                        }
                    case "6":
                        {
                            Console.WriteLine(LanguageHelper.GetMessage("Goodbye", language));
                            return;
                        }
                    default:
                        Console.WriteLine(LanguageHelper.GetMessage("InvalidChoice", language));
                        break;
                }
            }
        }

        /// <summary>
        /// Gère l'exécution de jobs via des arguments en ligne de commande
        /// (ex: "1" => job 1, "1-3" => jobs 1 à 3, "1;3" => job 1 et 3, etc.)
        /// </summary>
        private static void ExecuteBackupFromArgs(EasySaveFacade facade, string[] args)
        {
            var jobIndices = args[0]
                .Split(new char[] { '-', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out int index) ? index - 1 : -1)
                .Where(index => index >= 0)
                .ToList();

            if (jobIndices.Count == 0)
            {
                Console.WriteLine("⚠ Erreur : Aucun numéro de sauvegarde valide fourni.");
                return;
            }

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

        /// <summary>
        /// Crée un BackupJob via la console (en demandant les infos à l’utilisateur).
        /// Retourne null si des champs obligatoires sont vides.
        /// </summary>
        private static BackupJob? CreateJobFromConsole(string language)
        {
            Console.WriteLine("\n" + LanguageHelper.GetMessage("AddJobTitle", language));

            Console.Write(LanguageHelper.GetMessage("EnterJobName", language));
            var name = Console.ReadLine()?.Trim();

            Console.Write(LanguageHelper.GetMessage("EnterSourceDir", language));
            var sourceDirectory = Console.ReadLine()?.Trim();

            Console.Write(LanguageHelper.GetMessage("EnterTargetDir", language));
            var targetDirectory = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(sourceDirectory) || string.IsNullOrEmpty(targetDirectory))
            {
                Console.WriteLine(LanguageHelper.GetMessage("ErrorFieldsRequired", language));
                return null;
            }

            Console.WriteLine(LanguageHelper.GetMessage("SelectBackupType", language));
            Console.WriteLine(LanguageHelper.GetMessage("OptionComplete", language));
            Console.WriteLine(LanguageHelper.GetMessage("OptionDifferential", language));
            Console.Write("> ");
            var backupTypeInput = Console.ReadLine();
            BackupType backupType = backupTypeInput == "1" ? BackupType.Complete : BackupType.Differential;

            return new BackupJob(name, sourceDirectory, targetDirectory, backupType);
        }
    }
}
