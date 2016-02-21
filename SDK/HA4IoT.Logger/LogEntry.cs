using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Networking;
using HA4IoT.Networking;

namespace HA4IoT.Logger
{
    internal sealed class LogEntry : IExportToJsonValue
    {
        public LogEntry(long id, DateTime timestamp, int threadId, LogEntrySeverity severity, string message)
        {
            Id = id;
            Timestamp = timestamp;
            ThreadId = threadId;
            Severity = severity;
            Message = message;
        }

        public long Id { get; }

        public DateTime Timestamp { get; }

        public int ThreadId { get; }

        public LogEntrySeverity Severity { get; }

        public string Message { get; }

        public IJsonValue ExportToJsonObject()
        {
            var buffer = new JsonObject();
            buffer.SetNamedValue("Id", Id.ToJsonValue());
            buffer.SetNamedValue("Timestamp", Timestamp.ToJsonValue());
            buffer.SetNamedValue("ThreadId", ThreadId.ToJsonValue());
            buffer.SetNamedValue("Severity", Severity.ToJsonValue());
            buffer.SetNamedValue("Message", Message.ToJsonValue());

            return buffer;
        }
    }
}
