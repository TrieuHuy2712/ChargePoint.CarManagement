using System.Text;
using System.Text.Json;
using ChargePoint.CarManagement.Models;

namespace ChargePoint.CarManagement.Services.Logging;

public class FileLogStore : ILogStore
{
    private const int RetentionMonths = 3;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string _logFilePath;
    private readonly ILogger<FileLogStore> _logger;

    public FileLogStore(IWebHostEnvironment environment, ILogger<FileLogStore> logger)
    {
        _logger = logger;

        var logDir = Path.Combine(environment.ContentRootPath, "App_Data", "Logs");
        Directory.CreateDirectory(logDir);
        _logFilePath = Path.Combine(logDir, "system-log.jsonl");
    }

    public async Task WriteAsync(SystemLogEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var line = JsonSerializer.Serialize(entry, JsonOptions);
            var cutoffUtc = DateTime.UtcNow.AddMonths(-RetentionMonths);

            await _lock.WaitAsync(cancellationToken);
            try
            {
                await PruneExpiredEntriesUnsafeAsync(cutoffUtc, cancellationToken);
                await File.AppendAllTextAsync(_logFilePath, line + Environment.NewLine, Encoding.UTF8, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot write system log entry");
        }
    }

    public async Task<IReadOnlyList<SystemLogEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_logFilePath))
        {
            return [];
        }

        var result = new List<SystemLogEntry>();
        var cutoffUtc = DateTime.UtcNow.AddMonths(-RetentionMonths);
        string[] lines;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            await PruneExpiredEntriesUnsafeAsync(cutoffUtc, cancellationToken);

            if (!File.Exists(_logFilePath))
            {
                return [];
            }

            lines = await File.ReadAllLinesAsync(_logFilePath, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var item = JsonSerializer.Deserialize<SystemLogEntry>(line, JsonOptions);
                if (item != null && item.TimestampUtc >= cutoffUtc)
                {
                    result.Add(item);
                }
            }
            catch
            {
                // Skip invalid line
            }
        }

        return result
            .OrderByDescending(x => x.TimestampUtc)
            .ToList();
    }

    private async Task PruneExpiredEntriesUnsafeAsync(DateTime cutoffUtc, CancellationToken cancellationToken)
    {
        if (!File.Exists(_logFilePath))
        {
            return;
        }

        var lines = await File.ReadAllLinesAsync(_logFilePath, cancellationToken);
        if (lines.Length == 0)
        {
            return;
        }

        var keptLines = new List<string>(lines.Length);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var item = JsonSerializer.Deserialize<SystemLogEntry>(line, JsonOptions);
                if (item != null && item.TimestampUtc >= cutoffUtc)
                {
                    keptLines.Add(line);
                }
            }
            catch
            {
                // Skip invalid line
            }
        }

        if (keptLines.Count == 0)
        {
            File.Delete(_logFilePath);
            return;
        }

        if (keptLines.Count != lines.Length)
        {
            await File.WriteAllLinesAsync(_logFilePath, keptLines, Encoding.UTF8, cancellationToken);
        }
    }
}
