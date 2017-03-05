using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Logging
{
    public interface ILogService : IService
    {
        event EventHandler<LogEntryPublishedEventArgs> LogEntryPublished;
        
        ILogger CreatePublisher(string source);
    }
}
