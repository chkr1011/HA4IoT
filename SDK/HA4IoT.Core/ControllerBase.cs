using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using HA4IoT.Api;
using HA4IoT.Api.AzureCloud;
using HA4IoT.Api.LocalRestServer;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Logger;
using HA4IoT.Networking;
using HA4IoT.PersonalAgent;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Components;
using HA4IoT.Services.Devices;
using HA4IoT.Services.Health;
using HA4IoT.Services.Scheduling;
using HA4IoT.Services.System;
using HA4IoT.Telemetry;
using SimpleInjector;

namespace HA4IoT.Core
{
    public class ControllerBase : IController
    {
        private readonly Container _container = new Container();

        private BackgroundTaskDeferral _deferral;
        private HttpServer _httpServer;
        
        public ISettingsContainer Settings { get; private set; }

        public IInitializer Initializer { get; set; }

        protected ControllerBase(int? statusLedNumber)
        {
            _container.RegisterSingleton(() => new HealthServiceOptions {StatusLed = statusLedNumber});
            _container.RegisterSingleton<HealthService>();

            _container.RegisterSingleton<II2CBusService, BuiltInI2CBusService>();
            _container.RegisterSingleton<Pi2GpioService>();
            _container.RegisterSingleton<CCToolsBoardService>();
            
            _container.RegisterSingleton<IContainerService>(() => new ContainerService(_container));
            _container.RegisterSingleton<ISystemInformationService, SystemInformationService>();
            _container.RegisterSingleton<IDateTimeService, DateTimeService>();
            _container.RegisterSingleton<ISchedulerService, SchedulerService>();
            _container.RegisterSingleton<ITimerService, TimerService>();

            _container.RegisterSingleton<IAreaService, AreaService>();
            _container.RegisterSingleton<IDeviceService, DeviceService>();
            _container.RegisterSingleton<IComponentService, ComponentService>();
            _container.RegisterSingleton<IAutomationService, AutomationService>();

            _container.RegisterSingleton<IApiService, ApiService>();
            _container.RegisterSingleton<SynonymService>();
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
                InitializeCore,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            task.ConfigureAwait(false);
            return task;
        }
        
        protected virtual async Task ConfigureAsync(IContainerService factoryService)
        {
            await Task.FromResult(0);
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

        private void InitializeLogger()
        {
            if (Log.Instance == null)
            {
                var udpLogger = new UdpLogger();
                udpLogger.Start();

                udpLogger.ExposeToApi(_container.GetInstance<IApiService>());

                Log.Instance = udpLogger;
            }
            
            Log.Info("Starting...");
        }

        private void InitializeCore()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                InitializeLogger();
                InitializeHttpApiEndpoint();
                
                LoadControllerSettings();
                InitializeDiscovery();
                
                TryConfigure();
                
                LoadNonControllerSettings();
                ResetActuatorStates();

                StartHttpServer();

                ExposeToApi();

                AttachComponentHistoryTracking();

                foreach (var service in _container.GetAllInstances<IStartupCompletedNotification>())
                {
                    service.OnStartupCompleted();
                }
                
                stopwatch.Stop();
                Log.Info("Startup completed after " + stopwatch.Elapsed);
                
                _container.GetInstance<ISystemInformationService>().Set("Health/StartupDuration", stopwatch.Elapsed);
                _container.GetInstance<ISystemInformationService>().Set("Health/StartupTimestamp", DateTime.Now);
                _container.GetInstance<ITimerService>().Run();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to initialize.");
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

        private void InitializeDiscovery()
        {
            var discoveryServer = new DiscoveryServer(this);
            discoveryServer.Start();
        }

        private void LoadControllerSettings()
        {
            Settings = new SettingsContainer(StoragePath.WithFilename("ControllerConfiguration.json"));

            Settings.SetValue("Name", "HA4IoT Controller");
            Settings.SetValue("Description", "The HA4IoT controller which is responsible for this house.");
            Settings.SetValue("Language", "EN");

            Settings.Load();
            Settings.Save();
        }

        private void TryConfigure()
        {
            try
            {
                Log.Info("Starting configuration");
                Initializer?.RegisterServices();
                Initializer?.Initialize();

                ConfigureAsync(_container.GetInstance<IContainerService>()).Wait();
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
            _container.GetInstance<ControllerApiDispatcher>().ExposeToApi();

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

        // TODO: Migrate!

        ////private void CreateConfigurationStatistics()
        ////{
        ////    var systemInformationService = ServiceLocator.GetService<ISystemInformationService>();
        ////    systemInformationService.Set("Components/Count", _components.GetAll().Count);
        ////    systemInformationService.Set("Areas/Count", _areas.GetAll().Count);
        ////    systemInformationService.Set("Automations/Count", _automations.GetAll().Count);

        ////    systemInformationService.Set("Services/Count", ServiceLocator.GetServices().Count);
        ////}
    }
}
