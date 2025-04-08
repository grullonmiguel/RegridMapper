using Newtonsoft.Json;
using RegridMapper.Core.Configuration;
using System.IO;
using System.Text;

namespace RegridMapper.Core.Utilities
{
    /// <summary>
    /// Singleton Logger class for structured, asynchronous logging.
    /// Logs are stored in ProgramData/AppName/Logs and archived after 5 days.
    /// </summary>
    public sealed class Logger
    {
        private readonly string _logDirectory;
        private readonly string _logFilePath;
        private readonly string _archiveDirectory;
        private readonly List<LogEntry> _logEntries;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly Lazy<Logger> _instance = new(() => new Logger(AppConstants.AppName));

        /// <summary>
        /// Provides a single shared Logger instance.
        /// </summary>
        public static Logger Instance => _instance.Value;

        private Logger(string appName)
        {
            string programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), appName, "Logs");
            _logDirectory = programDataPath;
            _archiveDirectory = Path.Combine(programDataPath, "Archive");
            _logFilePath = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.json");

            Directory.CreateDirectory(_logDirectory);
            Directory.CreateDirectory(_archiveDirectory);

            _logEntries = LoadExistingLogs();

            Task.Run(async () => await CleanupOldLogsAsync()).ConfigureAwait(false);
        }

        private List<LogEntry> LoadExistingLogs()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    string existingContent = File.ReadAllText(_logFilePath);
                    return JsonConvert.DeserializeObject<List<LogEntry>>(existingContent) ?? new List<LogEntry>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading existing logs: {ex.Message}");
            }
            return new List<LogEntry>();
        }

        /// <summary>
        /// Logs a message asynchronously.
        /// </summary>
        public async Task LogAsync(string message, LogLevel level = LogLevel.Info)
        {
            var logEntry = new LogEntry(level, message);
            _logEntries.Add(logEntry);
            await SaveLogsAsync();
        }

        /// <summary>
        /// Logs an exception asynchronously.
        /// </summary>
        public async Task LogExceptionAsync(Exception ex) 
            => await LogAsync($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}", LogLevel.Error);

        /// <summary>
        /// Saves logs asynchronously to a JSON file.
        /// </summary>
        private async Task SaveLogsAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                string jsonContent = JsonConvert.SerializeObject(_logEntries, Formatting.Indented);
                await File.WriteAllTextAsync(_logFilePath, jsonContent, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving logs: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Cleans up old logs by archiving and deleting logs older than 5 days.
        /// </summary>
        private async Task CleanupOldLogsAsync()
        {
            try
            {
                foreach (var file in Directory.GetFiles(_logDirectory, "log_*.json"))
                {
                    var creationDate = File.GetCreationTime(file);
                    if ((DateTime.Now - creationDate).TotalDays > 5)
                    {
                        string archivePath = Path.Combine(_archiveDirectory, Path.GetFileName(file));
                        await Task.Run(() => File.Move(file, archivePath));
                    }
                }

                foreach (var file in Directory.GetFiles(_archiveDirectory, "log_*.json"))
                {
                    var creationDate = File.GetCreationTime(file);
                    if ((DateTime.Now - creationDate).TotalDays > 5)
                    {
                        await Task.Run(() => File.Delete(file));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during log cleanup: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Represents a structured log entry.
    /// </summary>
    public class LogEntry
    {
        public string Timestamp { get; }
        public string Level { get; }
        public string Message { get; }

        public LogEntry(LogLevel level, string message)
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Level = level.ToString();
            Message = message;
        }
    }

    /// <summary>
    /// Log level types (Info, Warning, Error).
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}