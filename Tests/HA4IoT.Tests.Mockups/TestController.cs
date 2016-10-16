using System;
using HA4IoT.Api;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services.Backup;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.Resources;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.Storage;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Notifications;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Backup;
using HA4IoT.Services.Components;
using HA4IoT.Services.Resources;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.StorageService;
using HA4IoT.Services.System;
using HA4IoT.Settings;

namespace HA4IoT.Tests.Mockups
{
    public class TestController : IController
    {
        public TestController()
        {
            Log.Instance = new TestLogger();

            // Create root services first.
            var systemInformationService = new SystemInformationService();
            var apiService = new ApiService();
            ApiService = new ApiService();
            BackupService = new BackupService();
            StorageService = new StorageService();
            TimerService = new TestTimerService();
            DaylightService = new TestDaylightService();
            DateTimeService = new TestDateTimeService();

            SettingsService = new SettingsService(BackupService, StorageService);
            ResourceService = new ResourceService(BackupService, StorageService, SettingsService);
            SchedulerService = new SchedulerService(TimerService, DateTimeService);
            NotificationService = new NotificationService(DateTimeService, ApiService, SchedulerService, SettingsService, StorageService, ResourceService);
            SystemEventsService = new SystemEventsService(this, NotificationService, ResourceService);
            AutomationService = new AutomationService(SystemEventsService, systemInformationService, apiService);
            ComponentService = new ComponentService(SystemEventsService, systemInformationService, apiService, SettingsService);
            AreaService = new AreaService(ComponentService, AutomationService, SystemEventsService, systemInformationService, apiService, SettingsService);
        }

        public ISettingsService SettingsService { get; }
        public IStorageService StorageService { get; }
        public IBackupService BackupService { get; }
        public IResourceService ResourceService { get; }
        public ISystemEventsService SystemEventsService { get; }
        public ITimerService TimerService { get; }
        public ISchedulerService SchedulerService { get; }
        public IDateTimeService DateTimeService { get; }
        public IDaylightService DaylightService { get; }
        public IComponentService ComponentService { get; }
        public IAutomationService AutomationService { get; }
        public INotificationService NotificationService { get; }
        public IAreaService AreaService { get; }
        public IApiService ApiService { get; }

        public event EventHandler StartupCompleted;
        public event EventHandler StartupFailed;
        public event EventHandler Shutdown;

        public void SetTime(TimeSpan value)
        {
            ((TestDateTimeService)DateTimeService).SetTime(value);
        }
    }
}
