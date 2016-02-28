using System;

namespace HA4IoT.TraceReceiver
{
    public class TraceItem
    {
        public TraceItem(
            long id,
            DateTime timestamp,
            int threadId,
            TraceItemSeverity severity,
            string message)
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

        public TraceItemSeverity Severity { get; }

        public string Message { get; }
    }
}
