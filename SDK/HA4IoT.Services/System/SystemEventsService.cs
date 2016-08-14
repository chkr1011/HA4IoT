using System;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.System
{
    public class SystemEventsService : ISystemEventsService
    {
        public event EventHandler StartupCompleted;

        public void FireStartupCompleted()
        {
            StartupCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
