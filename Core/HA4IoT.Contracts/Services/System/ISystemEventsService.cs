using System;

namespace HA4IoT.Contracts.Services.System
{
    public interface ISystemEventsService : IService
    {
        event EventHandler StartupCompleted;
        event EventHandler StartupFailed;
        event EventHandler Shutdown;
    }
}
