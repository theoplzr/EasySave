using Newtonsoft.Json;

namespace EasySaveLogs
{
    /// <summary>
    /// Singleton Logger class for handling log entries and writing them to JSON files.
    /// </summary>
    public sealed class Logger
    {
        private static Logger? _instance;
        private static readonly object _lock = new object();
        private readonly string _logDirectory;
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();

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
        private Logger(string logDirectory)
        {
            _logDirectory = logDirectory;
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Retrieves the single instance of the Logger class, creating it if necessary.
        /// </summary>
        /// <param name="logDirectory">The directory where log files will be stored.</param>
        /// <returns>The singleton instance of the Logger class.</returns>
        public static Logger GetInstance(string logDirectory)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new Logger(logDirectory);
                }
            }
            return _instance;
        }

        /// <summary>
        /// Logs an action by appending it to a JSON file.
        /// </summary>
        /// <param name="logEntry">The log entry to be recorded.</param>
        public void LogAction(LogEntry logEntry)
        {
            if (logEntry.Level < Level) return; // Filtering based on log level

            string logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            string json = JsonConvert.SerializeObject(logEntry, Formatting.None);
            File.AppendAllText(logFilePath, json + Environment.NewLine);
        }

        /// <summary>
        /// Flushes all stored log entries to a JSON file in an indented format.
        /// </summary>
        public void Flush()
        {
            string logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            File.WriteAllText(logFilePath, JsonConvert.SerializeObject(_logEntries, Formatting.Indented));
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
        /// Status of the backup operation (e.g., Success, Failed).
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// The log level associated with this entry.
        /// </summary>
        public Logger.LogLevel Level { get; set; } = Logger.LogLevel.Info;
    }
}
