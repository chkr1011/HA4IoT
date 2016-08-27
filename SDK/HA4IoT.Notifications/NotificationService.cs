using System;
using System.Collections.Generic;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Http;
using HA4IoT.Networking.Json;

namespace HA4IoT.Notifications
{
    [ApiServiceClass(typeof(INotificationService))]
    public class NotificationService : ServiceBase, INotificationService
    {
        private readonly object _syncRoot = new object();
        private readonly List<Notification> _notifications = new List<Notification>();
        private readonly IDateTimeService _dateTimeService;

        public NotificationService(IDateTimeService dateTimeService, IApiService apiService, ISchedulerService schedulerService, ISystemEventsService systemEventsService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            _dateTimeService = dateTimeService;

            apiService.StatusRequested += HandleApiStatusRequest;
            systemEventsService.StartupCompleted += (s, e) => CreateInformation("System started.");
            schedulerService.RegisterSchedule("NotificationCleanup", TimeSpan.FromMinutes(15), Cleanup);
        }

        public override void Startup()
        {
            lock (_syncRoot)
            {
                TryLoadNotifications();
            }
        }

        public void Create(NotificationType type, string message, TimeSpan timeToLive)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            lock (_syncRoot)
            {
                _notifications.Add(new Notification(Guid.NewGuid(), type, _dateTimeService.Now, message, timeToLive));
                SaveNotifications();
            }
        }

        public void CreateInformation(string text)
        {
            // TODO: TTL from settings service.
            Create(NotificationType.Information, text, TimeSpan.FromHours(1));
        }

        public void CreateWarning(string text)
        {
            // TODO: TTL from settings service.
            Create(NotificationType.Warning, text, TimeSpan.FromHours(24));
        }

        public void CreateError(string text)
        {
            // TODO: TTL from settings service.
            Create(NotificationType.Error, text, TimeSpan.FromHours(96));
        }

        [ApiMethod(ApiCallType.Request)]
        public void Notifications(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                apiContext.Response.SetNamedValue("Notifications", SerializeNotifications());
            }
        }

        [ApiMethod(ApiCallType.Command)]
        public void Delete(IApiContext apiContext)
        {
            var notificationUid = apiContext.Request.GetNamedString("Uid", string.Empty);
            if (string.IsNullOrEmpty(notificationUid))
            {
                throw new BadRequestException("Parameter 'Uid' is not specified.");
            }

            lock (_syncRoot)
            {
                var uid = Guid.Parse(notificationUid);
                var removedItems = _notifications.RemoveAll(n => n.Uid.Equals(uid));

                if (removedItems > 0)
                {
                    Log.Verbose($"Manually deleted notification '{notificationUid}'");
                    SaveNotifications();
                }
            }
        }

        private void Cleanup()
        {
            lock (_syncRoot)
            {
                Log.Verbose("Starting notification cleanup.");

                var now = _dateTimeService.Now;
                var removedItems = _notifications.RemoveAll(n => n.Timestamp.Add(n.TimeToLive) < now);

                Log.Verbose($"Deleted {removedItems} obsolete notifications.");
                if (removedItems > 0)
                {
                    SaveNotifications();
                }
            }
        }

        private void HandleApiStatusRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            lock (_syncRoot)
            {
                e.Context.Response.SetValue("Notifications", SerializeNotifications());
            }
        }

        private void SaveNotifications()
        {
            var jsonArray = SerializeNotifications();
            File.WriteAllText(StoragePath.WithFilename("NotificationService.json"), jsonArray.ToString());
        }

        private JsonArray SerializeNotifications()
        {
            var jsonArray = new JsonArray();
            foreach (var notification in _notifications)
            {
                jsonArray.Add(notification.ToJsonObject());
            }

            return jsonArray;
        }

        private void TryLoadNotifications()
        {
            try
            {
                var filename = StoragePath.WithFilename("NotificationService.json");
                if (!File.Exists(filename))
                {
                    return;
                }

                var fileContent = File.ReadAllText(filename);
                if (string.IsNullOrEmpty(fileContent))
                {
                    return;
                }

                JsonObject jsonObject;
                if (!JsonObject.TryParse(fileContent, out jsonObject))
                {
                    return;
                }

                var notifications = jsonObject.GetNamedArray("Notifications", new JsonArray());
                foreach (var notification in notifications)
                {
                    _notifications.Add(notification.ToObject<Notification>());
                }
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unable to load notifications.");
            }
        }
    }
}
