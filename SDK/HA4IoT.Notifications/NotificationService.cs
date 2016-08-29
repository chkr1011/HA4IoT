using System;
using System.Collections.Generic;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Settings;
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

        public NotificationService(IDateTimeService dateTimeService, IApiService apiService, ISchedulerService schedulerService, ISystemEventsService systemEventsService, ISettingsService settingsService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _dateTimeService = dateTimeService;
            Settings = settingsService.GetSettings<NotificationServiceSettings>();

            apiService.StatusRequested += HandleApiStatusRequest;
            systemEventsService.StartupCompleted += (s, e) => CreateInformation("System started.");
            schedulerService.RegisterSchedule("NotificationCleanup", TimeSpan.FromMinutes(15), Cleanup);
        }

        public NotificationServiceSettings Settings { get; }

        public void Initialize()
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
                var notification = new Notification
                {
                    Uid = Guid.NewGuid(),
                    Timestamp = _dateTimeService.Now,
                    Message = message,
                    TimeToLive = timeToLive,
                    Type = type
                };

                _notifications.Add(notification);

                SaveNotifications();
            }
        }

        public void CreateInformation(string text)
        {
            Create(NotificationType.Information, text, Settings.InformationTimeToLive);
        }

        public void CreateWarning(string text)
        {
            Create(NotificationType.Warning, text, Settings.WarningTimeToLive);
        }

        public void CreateError(string text)
        {
            Create(NotificationType.Error, text, Settings.ErrorTimeToLive);
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
            var jsonObject = new JsonObject();
            jsonObject.SetNamedValue("Notifications", SerializeNotifications());

            File.WriteAllText(StoragePath.WithFilename("NotificationService.json"), jsonObject.ToString());
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
                    _notifications.Add(notification.GetObject().DeserializeTo<Notification>());
                }
            }
            catch (Exception exception)
            {
                Fix error
                Log.Warning(exception, "Unable to load notifications.");
            }
        }
    }
}
