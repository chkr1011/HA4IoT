using System;

namespace HA4IoT.Logger
{
    internal sealed class LogEntry
    {
        public LogEntry(long id, DateTime timestamp, int threadId, LogEntrySeverity severity, string source, string message)
        {
            Id = id;
            Timestamp = timestamp;
            ThreadId = threadId;
            Severity = severity;

            Source = source;
            Message = message;
        }

        public long Id { get; }

        public DateTime Timestamp { get; }

        public int ThreadId { get; }

        public LogEntrySeverity Severity { get; }

        public string Source { get; }

        public string Message { get; }
    }
}
