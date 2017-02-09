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
using HA4IoT.Services.System;
using HA4IoT.Settings;
using SimpleInjector;

namespace HA4IoT.Tests.Mockups
{
    public class TestController : IController
    {
        private readonly Container _container = new Container();

        public TestController()
        {
            Log.Instance = new TestLogger();

            _container.RegisterSingleton<IController>(() => this);
            _container.RegisterSingleton<ISettingsService, SettingsService>();
            _container.RegisterSingleton<IApiDispatcherService, ApiDispatcherService>();
            _container.RegisterSingleton<ISystemInformationService, SystemInformationService>();
            _container.RegisterSingleton<IBackupService, BackupService>();
            _container.RegisterSingleton<IStorageService, TestStorageService>();
            _container.RegisterSingleton<ITimerService, TestTimerService>();
            _container.RegisterSingleton<IDaylightService, TestDaylightService>();
            _container.RegisterSingleton<IDateTimeService, TestDateTimeService>();
            _container.RegisterSingleton<IResourceService, ResourceService>();
            _container.RegisterSingleton<ISchedulerService, SchedulerService>();
            _container.RegisterSingleton<INotificationService, NotificationService>();
            _container.RegisterSingleton<ISystemEventsService, SystemEventsService>();
            _container.RegisterSingleton<IAutomationRegistryService, AutomationRegistryService>();
            _container.RegisterSingleton<IComponentRegistryService, ComponentRegistryService>();
            _container.RegisterSingleton<IAreaRegistryService, AreaRegistryService>();

            _container.Verify();
        }

        public event EventHandler StartupCompleted;
        public event EventHandler StartupFailed;
        public event EventHandler Shutdown;

        public TInstance GetInstance<TInstance>() where TInstance : class
        {
            return _container.GetInstance<TInstance>();
        }

        public void SetTime(TimeSpan value)
        {
            ((TestDateTimeService)GetInstance<IDateTimeService>()).SetTime(value);
        }
    }
}
