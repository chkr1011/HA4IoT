using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking;
using HA4IoT.Networking.Json;

namespace HA4IoT.Notifications
{
    [ApiServiceClass(typeof(INotificationService))]
    public class NotificationService : ServiceBase, INotificationService
    {
        private readonly IDateTimeService _dateTimeService;

        public NotificationService(IDateTimeService dateTimeService, IApiService apiService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            _dateTimeService = dateTimeService;

            apiService.StatusRequested += HandleApiStatusRequest;
        }

        private void HandleApiStatusRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            e.Context.Response.SetValue("notifications", new JsonObject());
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
