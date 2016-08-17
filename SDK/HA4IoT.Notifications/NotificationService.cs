using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Notifications
{
    public class NotificationService : ServiceBase, INotificationService
    {
        private readonly IDateTimeService _dateTimeService;

        public NotificationService(IDateTimeService dateTimeService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));

            _dateTimeService = dateTimeService;
        }

        public override void Startup()
        {
        }

        public void Create(NotificationType type, string text, TimeSpan timeToLive)
        {
            
        }

        public void CreateInformation(string text)
        {
            
        }

        public void CreateWarning(string text)
        {
            
        }

        public void CreateError(string text)
        {
            
        }

        [ApiMethod(ApiCallType.Request)]
        public void Notifications(IApiContext apiContext)
        {
            
        }

        [ApiMethod(ApiCallType.Command)]
        public void Delete(IApiContext apiContext)
        {
            
        }
    }
}
