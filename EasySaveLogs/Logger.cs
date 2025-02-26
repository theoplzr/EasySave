using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace EasySaveLogs
{
    /// <summary>
    /// A thread-safe singleton logger class responsible for writing log entries
    /// to either JSON or XML files. Allows dynamic reconfiguration of the log format.
    /// </summary>
    public sealed class Logger
    {
        private static Logger? _instance;
        private static readonly object _lock = new();
        private static readonly object _logFileLock = new();

        private readonly string _logDirectory;
        private string _logFormat;

        #region LogLevel Enum

        /// <summary>
        /// Represents the severity level of a log.
        /// </summary>
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        #endregion

        /// <summary>
        /// Defines the minimum level of logs that should be recorded.
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Info;

        /// <summary>
        /// Private constructor to enforce the singleton pattern.
        /// </summary>
        /// <param name="logDirectory">Directory where the log files will be stored.</param>
        /// <param name="logFormat">Format of the log files ("JSON" or "XML").</param>
        private Logger(string logDirectory, string logFormat)
        {
            string userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // If the user's home directory is available, default logs to that location
            if (string.IsNullOrWhiteSpace(userHomeDirectory))
            {
                _logDirectory = logDirectory;
            }
            else
            {
                _logDirectory = Path.Combine(userHomeDirectory, "Logs");
            }

            // Ensure that the log directory exists
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating log directory: {ex.Message}");
                // Fallback to the provided folder if we fail to create at the user's home
                _logDirectory = logDirectory;
            }

            _logFormat = logFormat;

            // Final check to ensure directory creation
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        #region Singleton Accessor

        /// <summary>
        /// Retrieves the single instance of the <see cref="Logger"/> class, creating it if necessary.
        /// </summary>
        /// <param name="logDirectory">The directory where log files will be stored.</param>
        /// <param name="logFormat">The format of the logs ("JSON" or "XML").</param>
        /// <returns>The singleton instance of the logger.</returns>
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

        #endregion

        #region Reconfiguration

        /// <summary>
        /// Dynamically reconfigures the logger with a new log format ("JSON" or "XML").
        /// </summary>
        /// <param name="newLogFormat">The new log format.</param>
        public void Reconfigure(string newLogFormat)
        {
            _logFormat = newLogFormat;
            Console.WriteLine($"Logger reconfigured to format: {_logFormat}");
        }

        #endregion

        #region Logging Methods

        /// <summary>
        /// Logs a <see cref="LogEntry"/> by writing it to the appropriate file format.
        /// Filters out entries that are below the configured <see cref="Level"/>.
        /// </summary>
        /// <param name="logEntry">The log entry to record.</param>
        public void LogAction(LogEntry logEntry)
        {
            if (logEntry.Level < Level)
                return; // Skip logging if below the log level threshold

            if (_logFormat.Equals("XML", StringComparison.OrdinalIgnoreCase))
            {
                LogToXml(logEntry);
            }
            else
            {
                LogToJson(logEntry);
            }
        }

        /// <summary>
        /// Writes a <see cref="LogEntry"/> to a JSON file, creating or appending to a file
        /// named according to the current timestamp (to the second).
        /// </summary>
        /// <param name="logEntry">The log entry to write.</param>
        private void LogToJson(LogEntry logEntry)
        {
            lock (_logFileLock)
            {
                // Use the current timestamp to create a unique file name
                string logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");
                var logEntries = new List<LogEntry>();

                if (File.Exists(logFilePath))
                {
                    string existingLogs = File.ReadAllText(logFilePath);
                    logEntries = JsonConvert.DeserializeObject<List<LogEntry>>(existingLogs) ?? new List<LogEntry>();
                }

                logEntries.Add(logEntry);
                string serialized = JsonConvert.SerializeObject(logEntries, Formatting.Indented);
                File.WriteAllText(logFilePath, serialized);
            }
        }

        /// <summary>
        /// Writes a <see cref="LogEntry"/> to an XML file, creating or appending to a file
        /// named according to the current timestamp (to the second).
        /// </summary>
        /// <param name="logEntry">The log entry to write.</param>
        private void LogToXml(LogEntry logEntry)
        {
            lock (_logFileLock)
            {
                // Use the current timestamp to create a unique file name
                string logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xml");
                XDocument xmlDoc;

                if (File.Exists(logFilePath))
                {
                    xmlDoc = XDocument.Load(logFilePath);
                }
                else
                {
                    xmlDoc = new XDocument(new XElement("Logs"));
                }

                xmlDoc.Root!.Add(
                    new XElement("LogEntry",
                        new XElement("Timestamp", logEntry.Timestamp),
                        new XElement("BackupName", logEntry.BackupName),
                        new XElement("SourceFilePath", logEntry.SourceFilePath),
                        new XElement("TargetFilePath", logEntry.TargetFilePath),
                        new XElement("FileSize", logEntry.FileSize),
                        new XElement("TransferTimeMs", logEntry.TransferTimeMs),
                        new XElement("EncryptionTimeMs", logEntry.EncryptionTimeMs),
                        new XElement("Status", logEntry.Status),
                        new XElement("Level", logEntry.Level.ToString())
                    )
                );

                xmlDoc.Save(logFilePath);
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a single log entry containing metadata about a backup action or event.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Timestamp when this log entry was generated.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The name of the backup operation.
        /// </summary>
        public string BackupName { get; set; } = string.Empty;

        /// <summary>
        /// The file path of the source file involved in this log event.
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The file path of the target file involved in this log event.
        /// </summary>
        public string TargetFilePath { get; set; } = string.Empty;

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// The time taken to transfer the file, in milliseconds.
        /// </summary>
        public long TransferTimeMs { get; set; }

        /// <summary>
        /// The time taken to encrypt the file, in milliseconds.
        /// </summary>
        public long EncryptionTimeMs { get; set; }

        /// <summary>
        /// A status message indicating the result of the backup or encryption operation.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The log level associated with this entry (Info, Warning, or Error).
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Logger.LogLevel Level { get; set; } = Logger.LogLevel.Info;
    }
}
