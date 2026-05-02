using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ChargePoint.CarManagement.Models;
using Microsoft.Extensions.Logging;

namespace ChargePoint.CarManagement.Services.Logging;

public sealed class SystemFileLoggerProvider(string logFilePath) : ILoggerProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly object _sync = new();
    private readonly string _logFilePath = logFilePath;

    public ILogger CreateLogger(string categoryName)
        => new SystemFileLogger(categoryName, _logFilePath, _sync);

    public void Dispose()
    {
        // no-op
    }

    private sealed class SystemFileLogger(string categoryName, string logFilePath, object sync) : ILogger
    {
        private readonly string _categoryName = categoryName;
        private readonly string _logFilePath = logFilePath;
        private readonly object _sync = sync;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // Giữ log của app để tránh quá nhiều framework logs
            if (!_categoryName.StartsWith("ChargePoint.", StringComparison.OrdinalIgnoreCase)
                && !_categoryName.Equals("Program", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message) && exception == null)
            {
                return;
            }

            var entry = new SystemLogEntry
            {
                TimestampUtc = DateTime.UtcNow,
                Level = logLevel.ToString(),
                Source = _categoryName,
                Message = string.IsNullOrWhiteSpace(message) ? (exception?.Message ?? "") : message,
                Detail = exception?.ToString(),
                TraceId = Activity.Current?.TraceId.ToString()
            };

            var line = JsonSerializer.Serialize(entry, JsonOptions);
            lock (_sync)
            {
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
