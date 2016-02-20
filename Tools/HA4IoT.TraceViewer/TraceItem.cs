using System;

namespace HA4IoT.TraceViewer
{
    public class TraceItem
    {
        public TraceItem(
            DateTime timestamp,
            int threadId,
            TraceItemSeverity type,
            string message)
        {
            Timestamp = timestamp;
            ThreadId = threadId;
            Type = type;
            Message = message;
        }

        public DateTime Timestamp { get; }

        public int ThreadId { get; }

        public TraceItemSeverity Type { get; }

        public string Message { get; }
    }
}
