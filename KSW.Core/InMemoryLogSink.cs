using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSW
{
    public class InMemoryLogSink : ILogEventSink, IDisposable
    {

        private readonly ConcurrentQueue<LogEntry> _entries = new();
        private const int MaxEntries = 2000;
        private readonly int _maxDisplayEntries;
        private readonly object _lock = new();

        public event Action? NewLogEntry;

        public InMemoryLogSink(int maxDisplayEntries = 500)
        {
            _maxDisplayEntries = maxDisplayEntries;
        }

        public void Emit(LogEvent logEvent)
        {
            var entry = new LogEntry(
                Timestamp: logEvent.Timestamp.LocalDateTime,
                Level: logEvent.Level.ToString(),
                SourceContext: GetSourceContext(logEvent),
                Message: logEvent.RenderMessage(),
                Exception: logEvent.Exception?.ToString(),
                LevelEnum: logEvent.Level);

            _entries.Enqueue(entry);

            while (_entries.Count > MaxEntries)
                _entries.TryDequeue(out _);

            NewLogEntry?.Invoke();
        }

        public IEnumerable<LogEntry> GetRecent(int count)
        {
            lock (_lock)
            {
                return _entries.TakeLast(count).ToList();
            }
        }

        public IEnumerable<LogEntry> GetFiltered(LogEventLevel? minLevel = null, string? keyword = null)
        {
            var query = _entries.AsEnumerable();

            if (minLevel.HasValue)
                query = query.Where(e => e.LevelEnum == minLevel.Value);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(e => e.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase) || (e.SourceContext?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));

            return query.TakeLast(_maxDisplayEntries);
        }

        private static string? GetSourceContext(LogEvent logEvent)
        {
            return logEvent.Properties.TryGetValue("SourceContext", out var value)
                ? value?.ToString().Trim('"')
                : null;
        }

        public void Clear() => _entries.Clear();

        public void Dispose() => Clear();
    }

    public record LogEntry(DateTime Timestamp, string Level, string? SourceContext, string Message, string? Exception, LogEventLevel LevelEnum);
}
