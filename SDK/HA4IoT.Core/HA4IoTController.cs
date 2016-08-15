using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using HA4IoT.Actuators;
using HA4IoT.Api;
using HA4IoT.Api.AzureCloud;
using HA4IoT.Api.LocalRestServer;
using HA4IoT.Automations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Logger;
using HA4IoT.Networking;
using HA4IoT.PersonalAgent;
using HA4IoT.Sensors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Components;
using HA4IoT.Services.Devices;
using HA4IoT.Services.Environment;
using HA4IoT.Services.Health;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.System;
using HA4IoT.Settings;
using HA4IoT.Telemetry;
using SimpleInjector;

namespace HA4IoT.Core
{
    public class HA4IoTController
    {
        private readonly Container _container = new Container();

        private readonly ControllerOptions _options;
        private readonly TimerService _timerService = new TimerService();
        private readonly SystemEventsService _systemEventsService = new SystemEventsService();
        private readonly SystemInformationService _systemInformationService = new SystemInformationService();
        private readonly ApiService _apiService = new ApiService();
        private readonly ContainerService _containerService;
        
        private BackgroundTaskDeferral _deferral;
        private HttpServer _httpServer;

        public HA4IoTController(ControllerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _options = options;
            
            _containerService = new ContainerService(_container);
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

        private void InitializeHttpApiEndpoint()
        {
            _httpServer = new HttpServer();

            var httpApiDispatcherEndpoint = new LocalHttpServerApiDispatcherEndpoint(_httpServer);
            _container.GetInstance<IApiService>().RegisterEndpoint(httpApiDispatcherEndpoint);

            var httpRequestDispatcher = new HttpRequestDispatcher(_httpServer);
            httpRequestDispatcher.MapFolder("App", StoragePath.AppRoot);
            httpRequestDispatcher.MapFolder("Storage", StoragePath.Root);
        }

        private bool TryInitializeAzureCloudApiEndpoint()
        {
            var azureCloudApiDispatcherEndpoint = new AzureCloudApiDispatcherEndpoint();

            if (azureCloudApiDispatcherEndpoint.TryInitializeFromConfigurationFile(
                StoragePath.WithFilename("AzureCloudApiDispatcherEndpointSettings.json")))
            {
                _container.GetInstance<IApiService>().RegisterEndpoint(azureCloudApiDispatcherEndpoint);

                return true;
            }

            return false;
        }

        private void Startup()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                UdpLogger udpLogger = null;
                if (Log.Instance == null)
                {
                    udpLogger = new UdpLogger();
                    udpLogger.Start();
                    Log.Instance = udpLogger;
                }

                Log.Info("Starting...");

                RegisterServices();

                InitializeHttpApiEndpoint();

                TryConfigure();

                LoadNonControllerSettings();
                ResetActuatorStates();

                StartHttpServer();

                ExposeToApi();
                udpLogger?.ExposeToApi(_container.GetInstance<IApiService>());

                AttachComponentHistoryTracking();

                _systemEventsService.FireStartupCompleted();
                stopwatch.Stop();

                Log.Info("Startup completed after " + stopwatch.Elapsed);

                _systemInformationService.Set("Health/StartupDuration", stopwatch.Elapsed);
                _systemInformationService.Set("Health/StartupTimestamp", DateTime.Now);
                _timerService.Run();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to initialize.");
            }
        }

        private void RegisterServices()
        {
            _container.RegisterSingleton<ControllerSettings>();

            _container.RegisterSingleton(() => new HealthServiceOptions { StatusLed = _options.StatusLedNumber });
            _container.RegisterSingleton<HealthService>();
            _container.RegisterSingleton<DiscoveryServer>();

            _container.RegisterSingleton<II2CBusService, BuiltInI2CBusService>();
            _container.RegisterSingleton<IPi2GpioService, Pi2GpioService>();
            _container.RegisterSingleton<CCToolsBoardService>();
            _container.RegisterSingleton<RemoteSocketService>();

            _container.RegisterSingleton<ITimerService>(() => _timerService);
            _container.RegisterSingleton<ISystemEventsService>(() => _systemEventsService);
            _container.RegisterSingleton<IContainerService>(() => _containerService);
            _container.RegisterSingleton<ISystemInformationService>(() => _systemInformationService);
            _container.RegisterSingleton<IApiService>(() => _apiService);
            _container.RegisterSingleton<IDateTimeService, DateTimeService>();
            _container.RegisterSingleton<ISchedulerService, SchedulerService>();
            
            _container.RegisterSingleton<IDeviceService, DeviceService>();
            _container.RegisterSingleton<IComponentService, ComponentService>();
            _container.RegisterSingleton<IAreaService, AreaService>();
            _container.RegisterSingleton<IAutomationService, AutomationService>();

            _container.RegisterSingleton<AutomationFactory>();
            _container.RegisterSingleton<ActuatorFactory>();
            _container.RegisterSingleton<SensorFactory>();

            _container.RegisterSingleton<PersonalAgentService>();
            _container.RegisterSingleton<SynonymService>();

            _container.RegisterSingleton<IOutdoorTemperatureService, OutdoorTemperatureService>();
            _container.RegisterSingleton<IOutdoorHumidityService, OutdoorHumidityService>();
            _container.RegisterSingleton<IDaylightService, DaylightService>();
            _container.RegisterSingleton<IWeatherService, WeatherService>();

            _options.Configurator?.RegisterServices(_containerService);

            _container.Verify();

            foreach (var registration in _containerService.GetCurrentRegistrations())
            {
                var apiExposedService = registration.GetInstance() as IApiExposedService;
                if (apiExposedService != null)
                {
                    _apiService.Route($"service/{registration.ServiceType.Name}", apiExposedService.HandleApiCall);
                }
            }
        }

        private void StartHttpServer()
        {
            try
            {
                _httpServer.Start(80);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while starting HTTP server on port 80. Falling back to port 55000");
                _httpServer.Start(55000);
            }
        }

        // TODO: To OnStartupCompleted
        private void ResetActuatorStates()
        {
            foreach (var actuator in _container.GetInstance<IComponentService>().GetComponents<IActuator>())
            {
                try
                {
                    actuator.ResetState();
                }
                catch (Exception exception)
                {
                    Log.Warning(exception, $"Error while initially reset of state for actuator '{actuator.Id}'.");
                }
            }
        }

        private void TryConfigure()
        {
            try
            {
                Log.Info("Starting configuration");
                _options.Configurator?.Configure(_containerService).Wait();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while configuring");
            }
        }

        private void LoadNonControllerSettings()
        {
            foreach (var area in _container.GetInstance<IAreaService>().GetAreas())
            {
                area.Settings.Load();
            }

            foreach (var component in _container.GetInstance<IComponentService>().GetComponents())
            {
                component.Settings.Load();
            }

            foreach (var automation in _container.GetInstance<IAutomationService>().GetAutomations())
            {
                automation.Settings.Load();
            }
        }

        private void ExposeToApi()
        {
            _apiService.ConfigurationRequested += (s, e) =>
            {
                e.Context.Response.SetNamedValue("controller", _container.GetInstance<ControllerSettings>().Export());
            };

            TryInitializeAzureCloudApiEndpoint();
        }

        private void AttachComponentHistoryTracking()
        {
            foreach (var component in _container.GetInstance<IComponentService>().GetComponents())
            {
                var history = new ComponentStateHistoryTracker(component);
                history.ExposeToApi(_container.GetInstance<IApiService>());
            }
        }
    }
}
