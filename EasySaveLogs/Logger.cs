using Newtonsoft.Json;

namespace EasySaveLogs
{
    public sealed class Logger
    {
        private static Logger? _instance;
        private static readonly object _lock = new object();
        private readonly string _logDirectory;
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();

        public enum LogLevel { Info, Warning, Error }
        public LogLevel Level { get; set; } = LogLevel.Info; 

        private Logger(string logDirectory)
        {
            _logDirectory = logDirectory;
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

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

        public void LogAction(LogEntry logEntry)
        {
            if (logEntry.Level < Level) return; // Filtrage par niveau

            string logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            string json = JsonConvert.SerializeObject(logEntry, Formatting.None);
            File.AppendAllText(logFilePath, json + Environment.NewLine);
        }

        public void Flush()
        {
            string logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            File.WriteAllText(logFilePath, JsonConvert.SerializeObject(_logEntries, Formatting.Indented));
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string BackupName { get; set; } = string.Empty;
        public string SourceFilePath { get; set; } = string.Empty;
        public string TargetFilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public long TransferTimeMs { get; set; }
        public string Status { get; set; } = string.Empty;
        public Logger.LogLevel Level { get; set; } = Logger.LogLevel.Info;
    }
}
