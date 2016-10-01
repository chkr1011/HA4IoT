using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.System
{
    public class SystemEventsService : ServiceBase, ISystemEventsService
    {
        public SystemEventsService(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            controller.StartupCompleted += (s, e) => StartupCompleted?.Invoke(this, EventArgs.Empty);
            controller.StartupFailed += (s, e) => StartupFailed?.Invoke(this, EventArgs.Empty);
            controller.Shutdown += (s, e) => Shutdown?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StartupCompleted;
        public event EventHandler StartupFailed;
        public event EventHandler Shutdown;
    }
}
