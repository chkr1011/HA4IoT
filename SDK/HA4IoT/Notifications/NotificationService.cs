using System;
using System.Collections.Generic;
using System.IO;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            settingsService.CreateSettingsMonitor<NotificationServiceSettings>(s => Settings = s);

            apiService.StatusRequested += HandleApiStatusRequest;
            systemEventsService.StartupCompleted += (s, e) => CreateInformation("System started.");

            schedulerService.RegisterSchedule("NotificationCleanup", TimeSpan.FromMinutes(15), Cleanup);
        }

        public NotificationServiceSettings Settings { get; private set; }

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
                var notification = new Notification(Guid.NewGuid(), type, _dateTimeService.Now, message, timeToLive);
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

        [ApiMethod]
        public void Delete(IApiContext apiContext)
        {
            var notificationUid = (string)apiContext.Request["Uid"];
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
                e.Context.Response["Notifications"] = JArray.FromObject(_notifications);
            }
        }

        private void SaveNotifications()
        {
            var filename = StoragePath.WithFilename("NotificationService.json");
            var content = JsonConvert.SerializeObject(_notifications);

            File.WriteAllText(filename, content);
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

                var notifications = JsonConvert.DeserializeObject<List<Notification>>(fileContent);
                _notifications.AddRange(notifications);
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unable to load notifications.");
            }
        }
    }
}
