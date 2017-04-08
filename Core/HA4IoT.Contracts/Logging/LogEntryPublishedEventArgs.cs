using System;

namespace HA4IoT.Contracts.Logging
{
    public class LogEntryPublishedEventArgs : EventArgs
    {
        public LogEntryPublishedEventArgs(LogEntry logEntry)
        {
            if (logEntry == null) throw new ArgumentNullException(nameof(logEntry));

            LogEntry = logEntry;
        }

        public LogEntry LogEntry { get; private set; }
    }
}
