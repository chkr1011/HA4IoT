using System;

namespace HA4IoT.Contracts.Core
{
    public interface IController
    {
        event EventHandler StartupCompleted;
        event EventHandler StartupFailed;
    }
}
