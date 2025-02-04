namespace EasySave.Core.Utils
{
    /// <summary>
    /// Singleton providing language translation support for UI messages.
    /// Supports English ("en") and French ("fr").
    /// </summary>
    public sealed class LanguageHelper
    {
        private static readonly Lazy<LanguageHelper> _instance =
            new Lazy<LanguageHelper>(() => new LanguageHelper());

        /// <summary>
        /// Gets the singleton instance of LanguageHelper.
        /// </summary>
        public static LanguageHelper Instance => _instance.Value;

        // English translations.
        private readonly Dictionary<string, string> _enDictionary = new Dictionary<string, string>
        {
            // Main menu options
            { "MenuTitle", "Backup Manager" },
            { "OptionAddJob", "1) Add a backup job" },
            { "OptionExecuteAll", "2) Execute all jobs" },
            { "OptionListJobs", "3) List all jobs" },
            { "OptionRemoveJob", "4) Remove a job" },
            { "OptionUpdateJob", "5) Update a job" },
            { "OptionExit", "6) Exit" },

            // Messages related to adding a backup job
            { "AddJobTitle", "--- Add a Backup Job ---" },
            { "EnterJobName", "Enter a name for the backup job: " },
            { "EnterSourceDir", "Enter the source directory: " },
            { "EnterTargetDir", "Enter the target directory: " },
            { "SelectBackupType", "Select the backup type:" },
            { "OptionComplete", "1) Complete" },
            { "OptionDifferential", "2) Differential" },
            { "ErrorFieldsRequired", "Error: All fields are required." },
            { "JobAdded", "Backup job '{0}' added successfully." },

            // Messages for updating a backup job
            { "EnterNewName", "Enter a new name (leave blank to keep existing): " },
            { "EnterNewSourceDir", "Enter a new source directory (leave blank to keep existing): " },
            { "EnterNewTargetDir", "Enter a new target directory (leave blank to keep existing): " },
            { "EnterNewBackupType", "Enter new backup type (1: Complete, 2: Differential), leave blank to keep existing: " },

            // Miscellaneous
            { "InvalidChoice", "Invalid choice. Please try again." },
            { "InvalidIndex", "Invalid index." },
            { "Goodbye", "Goodbye!" },
        };

        // French translations.
        private readonly Dictionary<string, string> _frDictionary = new Dictionary<string, string>
        {
            // Main menu options
            { "MenuTitle", "Gestionnaire de Sauvegarde" },
            { "OptionAddJob", "1) Ajouter un travail de sauvegarde" },
            { "OptionExecuteAll", "2) Exécuter tous les travaux de sauvegarde" },
            { "OptionListJobs", "3) Lister tous les travaux de sauvegarde" },
            { "OptionRemoveJob", "4) Supprimer un travail" },
            { "OptionUpdateJob", "5) Mettre à jour un travail" },
            { "OptionExit", "6) Quitter" },

            // Messages related to adding a backup job
            { "AddJobTitle", "--- Ajouter un travail de sauvegarde ---" },
            { "EnterJobName", "Entrez un nom pour le travail de sauvegarde : " },
            { "EnterSourceDir", "Entrez le répertoire source : " },
            { "EnterTargetDir", "Entrez le répertoire cible : " },
            { "SelectBackupType", "Sélectionnez le type de sauvegarde :" },
            { "OptionComplete", "1) Complète" },
            { "OptionDifferential", "2) Différentielle" },
            { "ErrorFieldsRequired", "Erreur : Tous les champs sont requis." },
            { "JobAdded", "Le travail de sauvegarde '{0}' a été ajouté avec succès." },

            // Messages for updating a backup job
            { "EnterNewName", "Entrez un nouveau nom (laisser vide pour conserver l'existant) : " },
            { "EnterNewSourceDir", "Entrez un nouveau répertoire source (laisser vide pour conserver l'existant) : " },
            { "EnterNewTargetDir", "Entrez un nouveau répertoire cible (laisser vide pour conserver l'existant) : " },
            { "EnterNewBackupType", "Entrez le nouveau type de sauvegarde (1 : Complète, 2 : Différentielle), laisser vide pour conserver l'existant : " },

            // Miscellaneous
            { "InvalidChoice", "Choix invalide. Veuillez réessayer." },
            { "InvalidIndex", "Index invalide." },
            { "Goodbye", "Au revoir !" },
        };

        // The currently active dictionary.
        private Dictionary<string, string> _currentDictionary;

        // The current language ("en" or "fr").
        private string _language;

        /// <summary>
        /// Private constructor to prevent direct instantiation.
        /// Sets the default language to English.
        /// </summary>
        private LanguageHelper()
        {
            _language = "en";
            _currentDictionary = _enDictionary;
        }

        /// <summary>
        /// Sets the current language and updates the dictionary accordingly.
        /// </summary>
        /// <param name="language">The language code ("en" or "fr").</param>
        public void SetLanguage(string language)
        {
            _language = language;
            _currentDictionary = language == "fr" ? _frDictionary : _enDictionary;
        }

        /// <summary>
        /// Retrieves the translated message for a given key.
        /// </summary>
        /// <param name="key">The key identifying the message (e.g., "MenuTitle", "OptionAddJob").</param>
        /// <returns>The translated message if found; otherwise, returns the key enclosed in brackets.</returns>
        public string GetMessage(string key)
        {
            if (_currentDictionary.TryGetValue(key, out string message))
            {
                return message;
            }
            // Fallback: return the key if no translation exists
            return $"[{key}]";
        }
    }
}
