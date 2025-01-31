using Newtonsoft.Json;

namespace EasySaveLogs
{
    /// <summary>
    /// Gère l'enregistrement des logs dans un fichier.
    /// Les logs sont stockés sous forme de liste dans un fichier JSON,
    /// chaque entrée de log représentant une action ou une erreur liée à la sauvegarde.
    /// </summary>
    public class Logger
    {
        private readonly string _logDirectory;

        public Logger(string logDirectory)
        {
            _logDirectory = logDirectory;

            // Crée le répertoire des logs si nécessaire
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Enregistre une action dans le fichier de log journalier.
        /// </summary>
        /// <param name="logEntry">Les données de l'action à enregistrer.</param>
        public void LogAction(LogEntry logEntry)
        {
            var logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");

            List<LogEntry> logEntries;

            // Si le fichier existe déjà, on charge les logs existants
            if (File.Exists(logFilePath))
            {
                var existingLogs = File.ReadAllText(logFilePath);
                logEntries = JsonConvert.DeserializeObject<List<LogEntry>>(existingLogs) ?? new List<LogEntry>();
            }
            else
            {
                logEntries = new List<LogEntry>();
            }

            // Ajoute la nouvelle entrée de log
            logEntries.Add(logEntry);

            // Sauvegarde les logs dans le fichier
            File.WriteAllText(logFilePath, JsonConvert.SerializeObject(logEntries, Formatting.Indented));
        }
    }

    /// <summary>
    /// Représente une entrée de log, contenant les informations relatives à une action de sauvegarde.
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string BackupName { get; set; } = string.Empty; // Valeur par défaut
        public string SourceFilePath { get; set; } = string.Empty; // Valeur par défaut
        public string TargetFilePath { get; set; } = string.Empty; // Valeur par défaut
        public long FileSize { get; set; }
        public long TransferTimeMs { get; set; }
        public string Status { get; set; } = string.Empty;  // Valeur par défaut
    }
}
