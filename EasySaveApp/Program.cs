﻿using Microsoft.Extensions.Configuration;
using EasySaveApp.Models;
using EasySaveApp.Utils;
using EasySaveApp.Observers;

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

            // Lire la configuration pour obtenir la langue et le répertoire des logs
            string language = configuration["Language"] ?? "en";
            string logDirectory = configuration["Logging:LogDirectory"] ?? "Logs";

            // Instanciation du BackupManager
            var backupManager = new BackupManager(logDirectory);

            // Instancier et enregistrer l'observer de fichier d'état
            // ettre à jour le fichier "state.json"
            string stateFilePath = "state.json"; // ou un chemin récupéré depuis la config si besoin
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
                Console.WriteLine(LanguageHelper.GetMessage("OptionAddJob", language));
                Console.WriteLine(LanguageHelper.GetMessage("OptionExecuteJobs", language));
                Console.WriteLine(LanguageHelper.GetMessage("OptionExit", language));
                Console.Write("> ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddBackupJob(backupManager, language);
                        break;
                    case "2":
                        backupManager.ExecuteAllJobs();
                        break;
                    case "3":
                        Console.WriteLine(LanguageHelper.GetMessage("Goodbye", language));
                        return;
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
        /// <param name="backupManager">Instance de BackupManager.</param>
        /// <param name="args">Les arguments de la ligne de commande.</param>
        private static void ExecuteBackupFromArgs(BackupManager backupManager, string[] args)
        {
            // On suppose que l'argument est fourni dans args[0]
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
                if (index < backupManager.GetBackupJobCount())
                {
                    backupManager.ExecuteBackupByIndex(index);
                }
                else
                {
                    Console.WriteLine($"⚠ Erreur : Index {index + 1} hors de portée.");
                }
            }
        }

        /// <summary>
        /// Permet à l'utilisateur d'ajouter un travail de sauvegarde via l'interface console.
        /// </summary>
        /// <param name="backupManager">Instance de BackupManager.</param>
        /// <param name="language">Langue sélectionnée (en/fr).</param>
        private static void AddBackupJob(BackupManager backupManager, string language)
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
                return;
            }

            Console.WriteLine(LanguageHelper.GetMessage("SelectBackupType", language));
            Console.WriteLine(LanguageHelper.GetMessage("OptionComplete", language));
            Console.WriteLine(LanguageHelper.GetMessage("OptionDifferential", language));
            Console.Write("> ");
            var backupTypeInput = Console.ReadLine();
            BackupType backupType = backupTypeInput == "1" ? BackupType.Complete : BackupType.Differential;

            var job = new BackupJob(name, sourceDirectory, targetDirectory, backupType);
            try
            {
                backupManager.AddBackupJob(job);
                Console.WriteLine(string.Format(LanguageHelper.GetMessage("JobAdded", language), name));
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"⚠ Erreur : {ex.Message}");
            }
        }
    }
}
