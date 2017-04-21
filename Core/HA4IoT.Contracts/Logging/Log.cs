using System;

namespace HA4IoT.Contracts.Logging
{
    public static class Log
    { 
        public static event EventHandler<LogEntryPublishedEventArgs> LogEntryPublished;

        public static ILogger Default { get; set; }

        public static void ForwardPublishedLogEntry(LogEntryPublishedEventArgs eventArgs)
        {
            LogEntryPublished?.Invoke(null, eventArgs);
        }
    }
}
