using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HA4IoT.Contracts.Logging
{
    public sealed class LogEntry
    {
        public LogEntry(DateTime timestamp, int threadId, LogEntrySeverity severity, string source, string message, string exception)
        {
            Timestamp = timestamp;
            ThreadId = threadId;
            Severity = severity;

            Source = source;
            Message = message;
            Exception = exception;
        }

        public long? Id { get; set; }

        public DateTime Timestamp { get; }

        public int ThreadId { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogEntrySeverity Severity { get; }

        public string Source { get; }

        public string Message { get; }

        public string Exception { get; }
    }
}
