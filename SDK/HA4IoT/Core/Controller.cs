using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using HA4IoT.Actuators;
using HA4IoT.Api;
using HA4IoT.Automations;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.ExternalServices.TelegramBot;
using HA4IoT.Contracts.Services.ExternalServices.Twitter;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Settings;
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
using HA4IoT.Logger;
using HA4IoT.Networking.Http;
using HA4IoT.Notifications;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Components;
using HA4IoT.Services.ControllerSlave;
using HA4IoT.Services.Devices;
using HA4IoT.Services.Environment;
using HA4IoT.Services.Health;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.System;
using HA4IoT.Settings;
using Newtonsoft.Json.Linq;
using SimpleInjector;

namespace HA4IoT.Core
{
    public class Controller : IController
    {
        private readonly Container _container = new Container();
        private readonly ControllerOptions _options;

        private BackgroundTaskDeferral _deferral;
        
        public Controller(ControllerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _options = options;
        }

        public Task RunAsync(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null) throw new ArgumentNullException(nameof(taskInstance));

            _deferral = taskInstance.GetDeferral();
            return RunAsync();
        }

        public Task RunAsync()
        {
            var task = Task.Factory.StartNew(
                Startup,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            task.ConfigureAwait(false);
            return task;
        }

        public event EventHandler StartupCompleted;
        public event EventHandler StartupFailed;
        public event EventHandler Shutdown; 

        private void Startup()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                SetupLogger();

                Log.Info("Starting...");

                RegisterServices();
                TryConfigure();

                StartupServices();
                ExposeRegistrationsToApi();
                StartHttpServer();

                StartupCompleted?.Invoke(this, EventArgs.Empty);
                stopwatch.Stop();

                Log.Info("Startup completed after " + stopwatch.Elapsed);

                _container.GetInstance<ISystemInformationService>().Set("Health/StartupDuration", stopwatch.Elapsed);
                _container.GetInstance<ISystemInformationService>().Set("Health/StartupTimestamp", _container.GetInstance<IDateTimeService>().Now);

                _container.GetInstance<ITimerService>().Run();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to initialize.");
                StartupFailed?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                Shutdown?.Invoke(this, EventArgs.Empty);
                _deferral?.Complete();
            }
        }

        private void StartHttpServer()
        {
            var httpServer = _container.GetInstance<HttpServer>();
            
            new MappedFolderController("App", StoragePath.AppRoot, httpServer).Enable();
            new MappedFolderController("ManagementApp", StoragePath.ManagementAppRoot, httpServer).Enable();

            httpServer.Bind(_options.HttpServerPort);
        }

        private void SetupLogger()
        {
            if (Log.Instance != null)
            {
                return;
            }

            var udpLogger = new UdpLogger();
            udpLogger.Start();
            
            Log.Instance = udpLogger;
        }

        private void StartupServices()
        {
            foreach (var registration in _container.GetRegistrationsOf<IService>())
            {
                ((IService)registration.GetInstance()).Startup();
            }
        }

        private void ExposeRegistrationsToApi()
        {
            var apiService = _container.GetInstance<IApiService>();
            var settingsService = _container.GetInstance<ISettingsService>();

            foreach (var registration in _container.GetCurrentRegistrations())
            {
                apiService.Expose(registration.GetInstance());
            }

            apiService.ConfigurationRequested += (s, e) =>
            {
                var controllerSettings = settingsService.GetSettings<ControllerSettings>();
                e.Context.Response["Controller"] = JObject.FromObject(controllerSettings);
            };
        }

        private void RegisterServices()
        {
            var containerService = new ContainerService(_container);

            _container.RegisterSingleton<IController>(() => this);
            _container.RegisterSingleton(() => Log.Instance);
            _container.RegisterSingleton<ControllerSettings>();

            _container.RegisterSingleton(() => new HealthServiceOptions { StatusLed = _options.StatusLedNumber });
            _container.RegisterSingleton<IHealthService, HealthService>();
            _container.RegisterSingleton<DiscoveryServerService>();
            _container.RegisterSingleton<HttpServer>();

            _container.RegisterSingleton<ITimerService, TimerService>();
            _container.RegisterSingleton<ISystemEventsService, SystemEventsService>();
            _container.RegisterSingleton<IContainerService>(() => containerService);
            _container.RegisterSingleton<ISystemInformationService, SystemInformationService>();

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
            
            _options.ContainerConfigurator?.ConfigureContainer(containerService);

            _container.Verify();
        }
        
        private void TryConfigure()
        {
            try
            {
                if (_options.ConfigurationType == null)
                {
                    Log.Warning("No configuration is set.");
                    return;
                }

                var configuration = _container.GetInstance(_options.ConfigurationType) as IConfiguration;
                if (configuration == null)
                {
                    Log.Warning("Configuration is set but does not implement 'IConfiguration'.");
                    return;
                }
                
                Log.Info("Applying configuration");
                configuration.ApplyAsync().Wait();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while configuring");

                _container.GetInstance<INotificationService>().CreateError("Configuration is invalid");
            }
        }
    }
}

