using System;

namespace HA4IoT.Contracts.Core
{
    public interface IController
    {
        event EventHandler<StartupCompletedEventArgs> StartupCompleted;
        event EventHandler<StartupFailedEventArgs> StartupFailed;
    }
}
