using System;

namespace HA4IoT.Contracts.Services.System
{
    public interface ISystemEventsService
    {
        event EventHandler StartupCompleted;
    }
}
