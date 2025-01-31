//classe pour gérer les langues à l'aide de dictionnaires

namespace EasySaveApp.Utils
{
    public static class LanguageHelper
    {
        // Dictionnaire pour l'anglais
        private static readonly Dictionary<string, string> enDictionary = new Dictionary<string, string>
        {
            { "MenuTitle", "Backup Manager" },
            { "OptionAddJob", "1) Add a backup job" },
            { "OptionExecuteJobs", "2) Execute all backup jobs" },
            { "OptionExit", "3) Exit" },
            { "AddJobTitle", "--- Add a Backup Job ---" },
            { "EnterJobName", "Enter a name for the backup job: " },
            { "EnterSourceDir", "Enter the source directory: " },
            { "EnterTargetDir", "Enter the target directory: " },
            { "SelectBackupType", "Select the backup type:" },
            { "OptionComplete", "1) Complete" },
            { "OptionDifferential", "2) Differential" },
            { "JobAdded", "Backup job '{0}' added successfully." },
            { "ErrorFieldsRequired", "Error: All fields are required." },
            { "Goodbye", "Goodbye!" },
            { "InvalidChoice", "Invalid choice. Please try again." }
        };

        // Dictionnaire pour le français
        private static readonly Dictionary<string, string> frDictionary = new Dictionary<string, string>
        {
            { "MenuTitle", "Gestionnaire de Sauvegarde" },
            { "OptionAddJob", "1) Ajouter un travail de sauvegarde" },
            { "OptionExecuteJobs", "2) Exécuter tous les travaux de sauvegarde" },
            { "OptionExit", "3) Quitter" },
            { "AddJobTitle", "--- Ajouter un travail de sauvegarde ---" },
            { "EnterJobName", "Entrez un nom pour le travail de sauvegarde : " },
            { "EnterSourceDir", "Entrez le répertoire source : " },
            { "EnterTargetDir", "Entrez le répertoire cible : " },
            { "SelectBackupType", "Sélectionnez le type de sauvegarde :" },
            { "OptionComplete", "1) Complète" },
            { "OptionDifferential", "2) Différentielle" },
            { "JobAdded", "Le travail de sauvegarde '{0}' a été ajouté avec succès." },
            { "ErrorFieldsRequired", "Erreur : Tous les champs sont requis." },
            { "Goodbye", "Au revoir !" },
            { "InvalidChoice", "Choix invalide. Veuillez réessayer." }
        };

        /// <summary>
        /// Récupère la chaîne de caractères correspondant à la clé, en fonction de la langue.
        /// </summary>
        /// <param name="key">La clé du message (ex: "MenuTitle", "Option1").</param>
        /// <param name="language">Code de la langue ("en" ou "fr").</param>
        /// <returns>Le message traduit.</returns>
        public static string GetMessage(string key, string language)
        {
            // Sélectionne le dictionnaire approprié selon la langue
            var dictionary = language == "fr" ? frDictionary : enDictionary;

            // Si la clé existe dans le dictionnaire, renvoie la traduction
            if (dictionary.ContainsKey(key))
                return dictionary[key];

            // Si la clé n'existe pas, renvoie la clé comme fallback
            return $"[{key}]";
        }
    }
}
