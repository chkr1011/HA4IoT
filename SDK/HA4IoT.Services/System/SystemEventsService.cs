using System;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.System
{
    public class SystemEventsService : ServiceBase, ISystemEventsService
    {
        public event EventHandler StartupCompleted;

        public void FireStartupCompleted()
        {
            StartupCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
