using System;
using Windows.Data.Json;

namespace HA4IoT.Logger
{
    internal sealed class LogEntry
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

        public JsonObject ToJsonObject()
        {
            var notification = new JsonObject();
            notification.SetNamedValue("timestamp", JsonValue.CreateStringValue(Timestamp.ToString("O")));
            notification.SetNamedValue("threadId", JsonValue.CreateStringValue(ThreadId.ToString()));
            notification.SetNamedValue("severity", JsonValue.CreateStringValue(Severity.ToString()));
            notification.SetNamedValue("message", JsonValue.CreateStringValue(Message));

            return notification;
        }
    }
}
