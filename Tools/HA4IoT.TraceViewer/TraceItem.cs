using System;

namespace HA4IoT.TraceViewer
{
    public class TraceItem
    {
        public TraceItem(
            long id,
            DateTime timestamp,
            int threadId,
            TraceItemSeverity type,
            string message)
        {
            Id = id;
            Timestamp = timestamp;
            ThreadId = threadId;
            Type = type;
            Message = message;
        }

        public long Id { get; }

        public DateTime Timestamp { get; }

        public int ThreadId { get; }

        public TraceItemSeverity Type { get; }

        public string Message { get; }
    }
}
