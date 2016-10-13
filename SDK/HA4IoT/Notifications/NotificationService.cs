using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Resources;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.Storage;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Http;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Notifications
{
    [ApiServiceClass(typeof(INotificationService))]
    public class NotificationService : ServiceBase, INotificationService
    {
        private const string StorageFilename = "NotificationService.json";

        private readonly object _syncRoot = new object();
        private readonly List<Notification> _notifications = new List<Notification>();
        private readonly IDateTimeService _dateTimeService;
        private readonly IStorageService _storageService;
        private readonly IResourceService _resourceService;

        public NotificationService(
            IDateTimeService dateTimeService, 
            IApiService apiService, 
            ISchedulerService schedulerService, 
            ISettingsService settingsService,
            IStorageService storageService,
            IResourceService resourceService)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (storageService == null) throw new ArgumentNullException(nameof(storageService));
            if (resourceService == null) throw new ArgumentNullException(nameof(resourceService));

            _dateTimeService = dateTimeService;
            _storageService = storageService;
            _resourceService = resourceService;

            settingsService.CreateSettingsMonitor<NotificationServiceSettings>(s => Settings = s);

            apiService.StatusRequested += HandleApiStatusRequest;

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

        public void CreateInformation(Enum resourceId, params object[] formatParameterObjects)
        {
            CreateInformation(_resourceService.GetText(resourceId, formatParameterObjects));
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
            _storageService.Write(StorageFilename, _notifications);
        }
        
        private void TryLoadNotifications()
        {
            List<Notification> persistedNotifications;
            if (_storageService.TryRead(StorageFilename, out persistedNotifications))
            {
                _notifications.AddRange(persistedNotifications);
            }
        }
    }
}
