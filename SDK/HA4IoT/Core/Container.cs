using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators;
using HA4IoT.Api;
using HA4IoT.Automations;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Backup;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.ExternalServices.TelegramBot;
using HA4IoT.Contracts.Services.ExternalServices.Twitter;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Resources;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.Storage;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.ExternalServices.AzureCloud;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.ExternalServices.TelegramBot;
using HA4IoT.ExternalServices.Twitter;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Networking.Http;
using HA4IoT.Notifications;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Backup;
using HA4IoT.Services.Components;
using HA4IoT.Services.ControllerSlave;
using HA4IoT.Services.Devices;
using HA4IoT.Services.Environment;
using HA4IoT.Services.Health;
using HA4IoT.Services.Resources;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.StorageService;
using HA4IoT.Services.System;
using HA4IoT.Settings;
using SimpleInjector;

namespace HA4IoT.Core
{
    public class Container : IContainer
    {
        private readonly SimpleInjector.Container _container = new SimpleInjector.Container();

        public void Verify()
        {
            _container.Verify();
        }

        public IList<InstanceProducer> GetCurrentRegistrations()
        {
            return _container.GetCurrentRegistrations().ToList();
        }

        public TInstance GetInstance<TInstance>() where TInstance : class
        {
            return _container.GetInstance<TInstance>();
        }

        public void RegisterSingleton<TConcrete>() where TConcrete : class
        {
            _container.RegisterSingleton<TConcrete>();
        }

        public void RegisterSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
        {
            _container.RegisterSingleton<TService, TImplementation>();
        }

        public void RegisterSingleton<TService>(Func<TService> instanceCreator) where TService : class
        {
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

            _container.RegisterSingleton(() => Log.Instance);
            _container.RegisterSingleton<ControllerSettings>();

            _container.RegisterSingleton<IHealthService, HealthService>();
            _container.RegisterSingleton<DiscoveryServerService>();
            _container.RegisterSingleton<HttpServer>();

            _container.RegisterSingleton<IStorageService, StorageService>();
            _container.RegisterSingleton<ITimerService, TimerService>();
            _container.RegisterSingleton<ISystemEventsService, SystemEventsService>();
            _container.RegisterSingleton<ISystemInformationService, SystemInformationService>();
            _container.RegisterSingleton<IBackupService, BackupService>();

            _container.RegisterSingleton<IResourceService, ResourceService>();
            _container.RegisterInitializer<ResourceService>(s => s.Initialize());

            _container.RegisterSingleton<IApiService, ApiService>();
            _container.RegisterSingleton<AzureCloudApiDispatcherEndpointService>();
            _container.RegisterSingleton<LocalHttpServerApiDispatcherEndpointService>();

            _container.RegisterSingleton<IDateTimeService, DateTimeService>();
            _container.RegisterSingleton<ISchedulerService, SchedulerService>();

            _container.RegisterSingleton<INotificationService, NotificationService>();
            _container.RegisterInitializer<NotificationService>(s => s.Initialize());

            _container.RegisterSingleton<ISettingsService, SettingsService>();
            _container.RegisterInitializer<SettingsService>(s => s.Initialize());

            _container.RegisterSingleton<II2CBusService, BuiltInI2CBusService>();
            _container.RegisterSingleton<IPi2GpioService, Pi2GpioService>();
            _container.RegisterSingleton<CCToolsBoardService>();
            _container.RegisterSingleton<RemoteSocketService>();

            _container.RegisterSingleton<IDeviceService, DeviceService>();

            _container.RegisterSingleton<IComponentService, ComponentService>();
            _container.RegisterSingleton<ActuatorFactory>();
            _container.RegisterSingleton<SensorFactory>();

            _container.RegisterSingleton<IAreaService, AreaService>();

            _container.RegisterSingleton<IAutomationService, AutomationService>();
            _container.RegisterSingleton<AutomationFactory>();

            _container.RegisterSingleton<IPersonalAgentService, PersonalAgentService>();
            _container.RegisterSingleton<SynonymService>();

            _container.RegisterSingleton<IOutdoorTemperatureService, OutdoorTemperatureService>();
            _container.RegisterSingleton<IOutdoorHumidityService, OutdoorHumidityService>();
            _container.RegisterSingleton<IDaylightService, DaylightService>();
            _container.RegisterSingleton<IWeatherService, WeatherService>();
            _container.RegisterSingleton<OpenWeatherMapService>();
            _container.RegisterSingleton<ControllerSlaveService>();

            _container.RegisterSingleton<ITwitterClientService, TwitterClientService>();
            _container.RegisterSingleton<ITelegramBotService, TelegramBotService>();
        }
    }
}
