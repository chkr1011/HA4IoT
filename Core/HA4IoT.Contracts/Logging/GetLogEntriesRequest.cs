using System;

namespace HA4IoT.Contracts.Logging
{
    public class GetLogEntriesRequest
    {
        public Guid? SessionId { get; set; }
        public int Offset { get; set; }
        public int MaxCount { get; set; }
    }
}
