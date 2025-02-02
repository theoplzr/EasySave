namespace EasySaveApp.Utils
{
    public static class LanguageHelper
    {
        // Dictionnaire pour l'anglais
        private static readonly Dictionary<string, string> enDictionary = new Dictionary<string, string>
        {
            // Menu principal
            { "MenuTitle", "Backup Manager" },
            { "OptionAddJob", "1) Add a backup job" },
            { "OptionExecuteAll", "2) Execute all jobs" },
            { "OptionListJobs", "3) List all jobs" },
            { "OptionRemoveJob", "4) Remove a job" },
            { "OptionUpdateJob", "5) Update a job" },
            { "OptionExit", "6) Exit" },

            // Messages liés à l'ajout d'un job
            { "AddJobTitle", "--- Add a Backup Job ---" },
            { "EnterJobName", "Enter a name for the backup job: " },
            { "EnterSourceDir", "Enter the source directory: " },
            { "EnterTargetDir", "Enter the target directory: " },
            { "SelectBackupType", "Select the backup type:" },
            { "OptionComplete", "1) Complete" },
            { "OptionDifferential", "2) Differential" },
            { "ErrorFieldsRequired", "Error: All fields are required." },
            { "JobAdded", "Backup job '{0}' added successfully." },

            // Messages pour la mise à jour d'un job
            { "EnterNewName", "Enter new name (leave blank to keep existing): " },
            { "EnterNewSourceDir", "Enter new source dir (leave blank to keep existing): " },
            { "EnterNewTargetDir", "Enter new target dir (leave blank to keep existing): " },
            { "EnterNewBackupType", "Enter new backup type (1: Complete, 2: Differential), leave blank to keep existing: " },

            // Divers
            { "InvalidChoice", "Invalid choice. Please try again." },
            { "InvalidIndex", "Invalid index." },
            { "Goodbye", "Goodbye!" },
        };

        // Dictionnaire pour le français
        private static readonly Dictionary<string, string> frDictionary = new Dictionary<string, string>
        {
            // Menu principal
            { "MenuTitle", "Gestionnaire de Sauvegarde" },
            { "OptionAddJob", "1) Ajouter un travail de sauvegarde" },
            { "OptionExecuteAll", "2) Exécuter tous les travaux de sauvegarde" },
            { "OptionListJobs", "3) Lister tous les travaux de sauvegarde" },
            { "OptionRemoveJob", "4) Supprimer un travail" },
            { "OptionUpdateJob", "5) Mettre à jour un travail" },
            { "OptionExit", "6) Quitter" },

            // Messages liés à l'ajout d'un job
            { "AddJobTitle", "--- Ajouter un travail de sauvegarde ---" },
            { "EnterJobName", "Entrez un nom pour le travail de sauvegarde : " },
            { "EnterSourceDir", "Entrez le répertoire source : " },
            { "EnterTargetDir", "Entrez le répertoire cible : " },
            { "SelectBackupType", "Sélectionnez le type de sauvegarde :" },
            { "OptionComplete", "1) Complète" },
            { "OptionDifferential", "2) Différentielle" },
            { "ErrorFieldsRequired", "Erreur : Tous les champs sont requis." },
            { "JobAdded", "Le travail de sauvegarde '{0}' a été ajouté avec succès." },

            // Messages pour la mise à jour d'un job
            { "EnterNewName", "Entrez un nouveau nom (laisser vide pour conserver l'existant) : " },
            { "EnterNewSourceDir", "Entrez un nouveau répertoire source (laisser vide pour conserver l'existant) : " },
            { "EnterNewTargetDir", "Entrez un nouveau répertoire cible (laisser vide pour conserver l'existant) : " },
            { "EnterNewBackupType", "Entrez le nouveau type de sauvegarde (1 : Complète, 2 : Différentielle), laisser vide pour conserver l'existant : " },

            // Divers
            { "InvalidChoice", "Choix invalide. Veuillez réessayer." },
            { "InvalidIndex", "Index invalide." },
            { "Goodbye", "Au revoir !" },
        };

        /// <summary>
        /// Récupère la chaîne de caractères correspondant à la clé, en fonction de la langue ("en" ou "fr").
        /// </summary>
        /// <param name="key">La clé du message (ex: "MenuTitle", "OptionAddJob").</param>
        /// <param name="language">Code de la langue ("en" ou "fr").</param>
        /// <returns>Le message traduit.</returns>
        public static string GetMessage(string key, string language)
        {
            var dictionary = (language == "fr") ? frDictionary : enDictionary;

            if (dictionary.ContainsKey(key))
                return dictionary[key];

            // Fallback : renvoyer la clé si elle n'existe pas
            return $"[{key}]";
        }
    }
}
