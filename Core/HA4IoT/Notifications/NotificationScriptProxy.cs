using System;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace HA4IoT.Notifications
{
    public class NotificationScriptProxy : IScriptProxy
    {
        private readonly INotificationService _notificationService;

        [MoonSharpHidden]
        public NotificationScriptProxy(INotificationService notificationService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        [MoonSharpHidden]
        public string Name => "notification";

        public void CreateInfo(string message)
        {
            _notificationService.CreateInfo(message);
        }

        public void CreateWarning(string message)
        {
            _notificationService.CreateWarning(message);
        }

        public void CreateError(string message)
        {
            _notificationService.CreateError(message);
        }
    }
}
