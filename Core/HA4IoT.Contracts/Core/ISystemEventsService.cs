using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface ISystemEventsService : IService
    {
        event EventHandler StartupCompleted;
        event EventHandler StartupFailed;
    }
}
