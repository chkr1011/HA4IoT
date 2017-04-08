using System;
using System.Collections.Generic;

namespace HA4IoT.Contracts.Logging
{
    public class GetLogEntriesResponse
    {
        public Guid SessionId { get; set; }

        public List<LogEntry> LogEntries { get; set; }
    }
}
