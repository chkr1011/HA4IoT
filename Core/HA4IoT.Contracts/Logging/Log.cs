using System;

namespace HA4IoT.Contracts.Logging
{
    public static class Log
    { 
        public static event EventHandler<LogEntryPublishedEventArgs> LogEntryPublished;

        public static ILogger Default { get; set; }

        public static void ForwardPublishedLogEntry(LogEntry logEntry)
        {
            LogEntryPublished?.Invoke(null, new LogEntryPublishedEventArgs(logEntry));
        }
    }
}
