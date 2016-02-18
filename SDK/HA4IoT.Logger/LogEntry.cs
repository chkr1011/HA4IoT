using System;
using Windows.Data.Json;
using HA4IoT.Networking;

namespace HA4IoT.Logger
{
    internal sealed class LogEntry : IExportToJsonValue
    {
        public LogEntry(DateTime timestamp, int threadId, LogEntrySeverity severity, string message)
        {
            Timestamp = timestamp;
            ThreadId = threadId;
            Severity = severity;
            Message = message;
        }

        public DateTime Timestamp { get; }

        public int ThreadId { get; }

        public LogEntrySeverity Severity { get; }

        public string Message { get; }

        public IJsonValue ExportToJsonObject()
        {
            var notification = new JsonObject();
            notification.SetNamedValue("timestamp", Timestamp.ToJsonValue());
            notification.SetNamedValue("threadId", ThreadId.ToJsonValue());
            notification.SetNamedValue("severity", Severity.ToJsonValue());
            notification.SetNamedValue("message", Message.ToJsonValue());

            return notification;
        }
    }
}
