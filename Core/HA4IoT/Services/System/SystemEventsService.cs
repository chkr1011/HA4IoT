using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Resources;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.System
{
    public class SystemEventsService : ServiceBase, ISystemEventsService
    {
        private readonly INotificationService _notificationService;

        public SystemEventsService(IController controller, INotificationService notificationService, IResourceService resourceService)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            controller.StartupCompleted += OnStartupCompleted;
            controller.StartupFailed += (s, e) => StartupFailed?.Invoke(this, EventArgs.Empty);

            resourceService.RegisterText(SystemEventNotification.Booted, "System is booted.");
        }

        public event EventHandler StartupCompleted;
        public event EventHandler StartupFailed;

        private void OnStartupCompleted(object sender, EventArgs eventArgs)
        {
            _notificationService.CreateInformation(SystemEventNotification.Booted);
            StartupCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
