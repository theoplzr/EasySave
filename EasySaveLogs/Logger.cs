using Newtonsoft.Json;

namespace EasySaveLogs
{
    /// <summary>
    /// Singleton Logger class for recording backup actions.
    /// Ensures that only one instance of the logger exists across the application.
    /// </summary>
    public sealed class Logger
    {
        /// <summary>
        /// The unique instance of the logger (Singleton pattern).
        /// </summary>
        private static Logger? _instance;

        /// <summary>
        /// Lock object to ensure thread-safety during instance creation.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Directory where log files are stored.
        /// </summary>
        private readonly string _logDirectory;

        /// <summary>
        /// Private constructor to prevent direct instantiation.
        /// Ensures that the log directory exists or creates it if necessary.
        /// </summary>
        /// <param name="logDirectory">The directory where log files will be stored.</param>
        private Logger(string logDirectory)
        {
            _logDirectory = logDirectory;

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Retrieves the single instance of the logger.
        /// Uses double-checked locking to optimize performance.
        /// </summary>
        /// <param name="logDirectory">The directory where logs will be stored (default: "Logs").</param>
        /// <returns>The singleton instance of <see cref="Logger"/>.</returns>
        public static Logger GetInstance(string logDirectory)
        {
            // Double-checked locking to minimize unnecessary locks in read scenarios
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
        /// Writes a backup action to the daily log file in JSON format.
        /// </summary>
        /// <param name="logEntry">The log entry containing details of the backup action.</param>
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
    /// Represents a log entry containing details of a backup action.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Timestamp of the logged backup action.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Name of the backup job associated with the log entry.
        /// </summary>
        public string BackupName { get; set; } = string.Empty;

        /// <summary>
        /// File path of the source file being backed up.
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        /// <summary>
        /// File path of the target (destination) file after backup.
        /// </summary>
        public string TargetFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file being backed up, in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Time taken to transfer the file, in milliseconds.
        /// </summary>
        public long TransferTimeMs { get; set; }

        /// <summary>
        /// Status of the backup action (e.g., "Success" or "Error").
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}
