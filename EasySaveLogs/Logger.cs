using Newtonsoft.Json;

namespace EasySaveLogs
{
    /// <summary>
    /// Logger géré en Singleton.
    /// </summary>
    public sealed class Logger
    {
        // L’instance unique
        private static Logger? _instance;

        // Objet lock pour la thread-safety
        private static readonly object _lock = new object();

        private readonly string _logDirectory;

        /// <summary>
        /// Constructeur privé pour empêcher l’instanciation directe.
        /// </summary>
        private Logger(string logDirectory)
        {
            _logDirectory = logDirectory;

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Propriété statique pour récupérer l’instance unique du logger.
        /// On peut prévoir un paramètre logDirectory si besoin.
        /// </summary>
        public static Logger GetInstance(string logDirectory = "Logs")
        {
            // Double-checked locking pour limiter les lock en lecture
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger(logDirectory);
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Méthode pour écrire une action dans le fichier log journalier.
        /// </summary>
        public void LogAction(LogEntry logEntry)
        {
            var logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");

            List<LogEntry> logEntries;
            if (File.Exists(logFilePath))
            {
                var existingLogs = File.ReadAllText(logFilePath);
                logEntries = JsonConvert.DeserializeObject<List<LogEntry>>(existingLogs) ?? new List<LogEntry>();
            }
            else
            {
                logEntries = new List<LogEntry>();
            }

            logEntries.Add(logEntry);

            File.WriteAllText(logFilePath, JsonConvert.SerializeObject(logEntries, Formatting.Indented));
        }
    }

    /// <summary>
    /// Représente une entrée de log, contient les infos sur une action de sauvegarde.
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string BackupName { get; set; } = string.Empty;
        public string SourceFilePath { get; set; } = string.Empty;
        public string TargetFilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public long TransferTimeMs { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
