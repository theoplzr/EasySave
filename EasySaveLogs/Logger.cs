using Newtonsoft.Json;
using System.Xml.Linq;

namespace EasySaveLogs
{
    /// <summary>
    /// Singleton Logger class for handling log entries and writing them to JSON or XML files.
    /// </summary>
    public sealed class Logger
    {
        private static Logger? _instance;
        private static readonly object _lock = new object();
        private readonly string _logDirectory;
        private string _logFormat; 

        /// <summary>
        /// Enum representing log levels.
        /// </summary>
        public enum LogLevel { Info, Warning, Error }

        /// <summary>
        /// Defines the minimum level of logs that should be recorded.
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Info;

        /// <summary>
        /// Private constructor for the Logger class to enforce singleton pattern.
        /// </summary>
        /// <param name="logDirectory">Directory where log files will be stored.</param>
        /// <param name="logFormat">Format of the log files (JSON or XML).</param>
        private Logger(string logDirectory, string logFormat)
        {
            string userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Vérifier si le chemin récupéré est vide ou null; utiliser logDirectory en fallback
            if (string.IsNullOrWhiteSpace(userHomeDirectory))
            {
                _logDirectory = logDirectory;
            }
            else
            {
                _logDirectory = System.IO.Path.Combine(userHomeDirectory, "Logs");
            }
            // Vérifier et créer le dossier si nécessaire
            try
            {
                if (!System.IO.Directory.Exists(_logDirectory))
                {
                    System.IO.Directory.CreateDirectory(_logDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création du dossier de logs : {ex.Message}");
                _logDirectory = logDirectory; // Fallback vers le dossier spécifié 
            }
            _logFormat = logFormat;

            if (!System.IO.Directory.Exists(_logDirectory))
            {
                System.IO.Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Retrieves the single instance of the Logger class, creating it if necessary.
        /// </summary>
        /// <param name="logDirectory">The directory where log files will be stored.</param>
        /// <param name="logFormat">Format of the log files (JSON or XML).</param>
        /// <returns>The singleton instance of the Logger class.</returns>
        public static Logger GetInstance(string logDirectory, string logFormat)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger(logDirectory, logFormat);
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Permet de reconfigurer dynamiquement le format de log.
        /// </summary>
        /// <param name="newLogFormat">Le nouveau format (JSON ou XML).</param>
        public void Reconfigure(string newLogFormat)
        {
            _logFormat = newLogFormat;
            Console.WriteLine($"Logger reconfiguré pour le format {_logFormat}");
        }

        /// <summary>
        /// Logs an action by appending it to a JSON or XML file.
        /// </summary>
        /// <param name="logEntry">The log entry to be recorded.</param>
        public void LogAction(LogEntry logEntry)
        {
            if (logEntry.Level < Level) return; // Filtrage basé sur le niveau du log

            if (_logFormat.ToUpper() == "XML")
                LogToXml(logEntry);
            else
                LogToJson(logEntry);
        }

        /// <summary>
        /// Logs an action to a JSON file.
        /// </summary>
        /// <param name="logEntry">The log entry to be recorded.</param>
        private void LogToJson(LogEntry logEntry)
        {
            string logFilePath = System.IO.Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            var logEntries = new List<LogEntry>();

            if (System.IO.File.Exists(logFilePath))
            {
                var existingLogs = System.IO.File.ReadAllText(logFilePath);
                logEntries = JsonConvert.DeserializeObject<List<LogEntry>>(existingLogs) ?? new List<LogEntry>();
            }

            logEntries.Add(logEntry);
            System.IO.File.WriteAllText(logFilePath, JsonConvert.SerializeObject(logEntries, Formatting.Indented));
        }

        /// <summary>
        /// Logs an action to an XML file.
        /// </summary>
        /// <param name="logEntry">The log entry to be recorded.</param>
        private void LogToXml(LogEntry logEntry)
        {
            string logFilePath = System.IO.Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.xml");
            XDocument xmlDoc;

            if (System.IO.File.Exists(logFilePath))
                xmlDoc = XDocument.Load(logFilePath);
            else
                xmlDoc = new XDocument(new XElement("Logs"));

            // Ajout de l'élément EncryptionTimeMs pour consigner le temps de cryptage
            xmlDoc.Root!.Add(new XElement("LogEntry",
                new XElement("Timestamp", logEntry.Timestamp),
                new XElement("BackupName", logEntry.BackupName),
                new XElement("SourceFilePath", logEntry.SourceFilePath),
                new XElement("TargetFilePath", logEntry.TargetFilePath),
                new XElement("FileSize", logEntry.FileSize),
                new XElement("TransferTimeMs", logEntry.TransferTimeMs),
                new XElement("EncryptionTimeMs", logEntry.EncryptionTimeMs),
                new XElement("Status", logEntry.Status),
                new XElement("Level", logEntry.Level.ToString())
            ));

            xmlDoc.Save(logFilePath);
        }
    }

    /// <summary>
    /// Represents a single log entry with metadata.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Timestamp when the log entry was created.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Name of the backup operation.
        /// </summary>
        public string BackupName { get; set; } = string.Empty;

        /// <summary>
        /// Path of the source file.
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Path of the target file.
        /// </summary>
        public string TargetFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file being backed up.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Time taken to transfer the file (in milliseconds).
        /// </summary>
        public long TransferTimeMs { get; set; }

        /// <summary>
        /// Time taken to encrypt the file (in milliseconds).
        /// </summary>
        public long EncryptionTimeMs { get; set; }

        /// <summary>
        /// Status of the backup operation (e.g., Success, Failed).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The log level associated with this entry.
        /// </summary>
        public Logger.LogLevel Level { get; set; } = Logger.LogLevel.Info;
    }
}
