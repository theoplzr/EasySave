using Microsoft.Extensions.Configuration;
using EasySaveApp.Models;
using EasySaveApp.Utils;
using EasySaveApp.Observers;
using EasySaveApp.Commands; 

namespace EasySaveApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Charger la configuration depuis le fichier appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            // Lire la configuration pour la langue et le répertoire des logs
            string language = configuration["Language"] ?? "en";
            string logDirectory = configuration["Logging:LogDirectory"] ?? "Logs";

            // Instanciation du BackupManager
            var backupManager = new BackupManager(logDirectory);

            // Instancier et enregistrer l'observer de fichier d'état
            string stateFilePath = "state.json"; 
            var fileStateObserver = new FileStateObserver(stateFilePath);
            backupManager.AddObserver(fileStateObserver);

            // Vérifier si des arguments en ligne de commande sont passés
            if (args.Length > 0)
            {
                ExecuteBackupFromArgs(backupManager, args);
                return;
            }

            // Demander à l'utilisateur de choisir la langue
            Console.Write("Choose language (en/fr): ");
            var userInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(userInput) && (userInput == "fr" || userInput == "en"))
            {
                language = userInput;
            }

            // Boucle principale de l'application
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
                            // On crée un nouveau job depuis la console
                            var job = CreateJobFromConsole(language);
                            if (job != null)
                            {
                                // On exécute la commande AddJobCommand
                                var addCmd = new AddJobCommand(backupManager, job);
                                addCmd.Execute();
                            }
                            break;
                        }
                    case "2":
                        {
                            // Commande pour exécuter tous les jobs
                            var executeAllCmd = new ExecuteAllJobsCommand(backupManager);
                            executeAllCmd.Execute();
                            break;
                        }
                    case "3":
                        {
                            // Commande pour lister tous les jobs
                            var listCmd = new ListJobsCommand(backupManager);
                            listCmd.Execute();
                            break;
                        }
                    case "4":
                        {
                            // Commande pour supprimer un job
                            Console.Write("Enter the index of the job to remove: ");
                            if (int.TryParse(Console.ReadLine(), out int removeIndex))
                            {
                                removeIndex -= 1; // Conversion en zéro-based
                                var removeCmd = new RemoveJobCommand(backupManager, removeIndex);
                                removeCmd.Execute();
                            }
                            else
                            {
                                Console.WriteLine("Invalid index.");
                            }
                            break;
                        }
                    case "5":
                        {
                            // Commande pour mettre à jour un job
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

                                var updateCmd = new UpdateJobCommand(backupManager, updateIndex, newName, newSource, newTarget, newType);
                                updateCmd.Execute();
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
        /// Exécute des sauvegardes via la ligne de commande en interprétant l'argument.
        /// Les formats acceptés sont :
        /// - "1"    : exécuter le job numéro 1
        /// - "1-3"  : exécuter les jobs 1, 2 et 3
        /// - "1;3"  : exécuter les jobs 1 et 3
        /// </summary>
        private static void ExecuteBackupFromArgs(BackupManager backupManager, string[] args)
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

            // Pour chaque index validé, on exécute un ExecuteJobCommand
            foreach (var index in jobIndices)
            {
                if (index < backupManager.GetBackupJobCount())
                {
                    var cmd = new ExecuteJobCommand(backupManager, index);
                    cmd.Execute();
                }
                else
                {
                    Console.WriteLine($"⚠ Erreur : Index {index + 1} hors de portée.");
                }
            }
        }

        /// <summary>
        /// Crée un BackupJob via la console (demande les informations au user).
        /// Renvoie null si des champs obligatoires sont vides.
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
