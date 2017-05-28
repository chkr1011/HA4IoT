using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HA4IoT.Actuators;
using HA4IoT.Api;
using HA4IoT.Api.Cloud.Azure;
using HA4IoT.Api.Cloud.CloudConnector;
using HA4IoT.Areas;
using HA4IoT.Automations;
using HA4IoT.Backup;
using HA4IoT.Components;
using HA4IoT.Configuration;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Backup;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Environment;
using HA4IoT.Contracts.ExternalServices.TelegramBot;
using HA4IoT.Contracts.ExternalServices.Twitter;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Interrupts;
using HA4IoT.Contracts.Hardware.RaspberryPi;
using HA4IoT.Contracts.Hardware.RemoteSockets;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.Resources;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Settings;
using HA4IoT.Contracts.Storage;
using HA4IoT.Devices;
using HA4IoT.Environment;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.ExternalServices.TelegramBot;
using HA4IoT.ExternalServices.Twitter;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2C;
using HA4IoT.Hardware.Interrupts;
using HA4IoT.Hardware.Outpost;
using HA4IoT.Hardware.RaspberryPi;
using HA4IoT.Hardware.RemoteSockets;
using HA4IoT.Hardware.Sonoff;
using HA4IoT.Health;
using HA4IoT.Logging;
using HA4IoT.Messaging;
using HA4IoT.Notifications;
using HA4IoT.PersonalAgent;
using HA4IoT.Resources;
using HA4IoT.Scheduling;
using HA4IoT.Scripting;
using HA4IoT.Sensors;
using HA4IoT.Settings;
using HA4IoT.Status;
using HA4IoT.Storage;
using SimpleInjector;

namespace HA4IoT.Core
{
    public class Container : IContainer
    {
        private readonly SimpleInjector.Container _container = new SimpleInjector.Container();
        private readonly ControllerOptions _options;

        public Container(ControllerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _container.RegisterSingleton(options);
        }

        public void Verify()
        {
            _container.Verify();
        }

        public IList<InstanceProducer> GetCurrentRegistrations()
        {
            return _container.GetCurrentRegistrations().ToList();
        }

        public TContract GetInstance<TContract>() where TContract : class
        {
            return _container.GetInstance<TContract>();
        }

        public IList<TContract> GetInstances<TContract>() where TContract : class
        {
            var services = new List<TContract>();

            foreach (var registration in _container.GetCurrentRegistrations())
            {
                if (typeof(TContract).IsAssignableFrom(registration.ServiceType))
                {
                    services.Add((TContract)registration.GetInstance());
                }
            }

            return services;
        }

        public void RegisterSingleton<TImplementation>() where TImplementation : class
        {
            _container.RegisterSingleton<TImplementation>();
        }

        public void RegisterSingleton<TContract, TImplementation>() where TContract : class where TImplementation : class, TContract
        {
            _container.RegisterSingleton<TContract, TImplementation>();
        }

        public void RegisterSingletonCollection<TItem>(IEnumerable<TItem> items) where TItem : class
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            _container.RegisterCollection(items);
        }

        public void RegisterSingleton<TContract>(Func<TContract> instanceCreator) where TContract : class
        {
            if (instanceCreator == null) throw new ArgumentNullException(nameof(instanceCreator));

            _container.RegisterSingleton(instanceCreator);
        }

        public object GetInstance(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return _container.GetInstance(type);
        }

        public void RegisterServices()
        {
            _container.RegisterSingleton<IContainer>(() => this);

            foreach (var customService in _options.CustomServices)
            {
                _container.RegisterSingleton(customService);
            }

            _container.RegisterCollection<ILogAdapter>(_options.LogAdapters);
            _container.RegisterSingleton<ILogService, LogService>();
            _container.RegisterSingleton<IHealthService, HealthService>();
            _container.RegisterSingleton<IDateTimeService, DateTimeService>();
            _container.RegisterSingleton<ISchedulerService, SchedulerService>();
            _container.RegisterSingleton<DiscoveryServerService>();

            _container.RegisterSingleton<IConfigurationService, ConfigurationService>();
            _container.RegisterInitializer<ConfigurationService>(s => s.Initialize());

            _container.RegisterSingleton<IStorageService, StorageService>();
            _container.RegisterSingleton<ITimerService, TimerService>();
            _container.RegisterSingleton<ISystemEventsService, SystemEventsService>();
            _container.RegisterSingleton<ISystemInformationService, SystemInformationService>();
            _container.RegisterSingleton<IBackupService, BackupService>();

            _container.RegisterSingleton<IResourceService, ResourceService>();
            _container.RegisterInitializer<ResourceService>(s => s.Initialize());

            _container.RegisterSingleton<IApiDispatcherService, ApiDispatcherService>();
            _container.RegisterSingleton<HttpServerService>();
            _container.RegisterSingleton<AzureCloudService>();
            _container.RegisterSingleton<CloudConnectorService>();

            _container.RegisterSingleton<INotificationService, NotificationService>();
            _container.RegisterInitializer<NotificationService>(s => s.Initialize());

            _container.RegisterSingleton<ISettingsService, SettingsService>();
            _container.RegisterInitializer<SettingsService>(s => s.Initialize());

            _container.RegisterSingleton<II2CBusService, I2CBusService>();
            _container.RegisterSingleton<IGpioService, GpioService>();
            _container.RegisterSingleton<IMessageBrokerService, MessageBrokerService>();
            _container.RegisterSingleton<IInterruptMonitorService, InterruptMonitorService>();

            _container.RegisterSingleton<IDeviceMessageBrokerService, DeviceMessageBrokerService>();
            _container.RegisterInitializer<DeviceMessageBrokerService>(s => s.Initialize());

            _container.RegisterSingleton<IRemoteSocketService, RemoteSocketService>();

            _container.RegisterSingleton<CCToolsDeviceService>();
            _container.RegisterSingleton<SonoffDeviceService>();
            _container.RegisterSingleton<OutpostDeviceService>();

            _container.RegisterSingleton<IDeviceRegistryService, DeviceRegistryService>();
            _container.RegisterSingleton<IAreaRegistryService, AreaRegistryService>();
            _container.RegisterSingleton<IComponentRegistryService, ComponentRegistryService>();
            _container.RegisterSingleton<IAutomationRegistryService, AutomationRegistryService>();
            _container.RegisterSingleton<IScriptingService, ScriptingService>();

            _container.RegisterSingleton<ActuatorFactory>();
            _container.RegisterSingleton<SensorFactory>();
            _container.RegisterSingleton<AutomationFactory>();

            _container.RegisterSingleton<IPersonalAgentService, PersonalAgentService>();

            _container.RegisterSingleton<IOutdoorService, OutdoorService>();
            _container.RegisterSingleton<IDaylightService, DaylightService>();
            _container.RegisterSingleton<OpenWeatherMapService>();
            _container.RegisterSingleton<ControllerSlaveService>();

            _container.RegisterSingleton<ITwitterClientService, TwitterClientService>();
            _container.RegisterSingleton<ITelegramBotService, TelegramBotService>();

            _container.RegisterSingleton<IStatusService, StatusService>();
        }
    }
}
