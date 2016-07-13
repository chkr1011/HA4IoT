using System;
using System.Collections.Generic;
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
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Core.Discovery;
using HA4IoT.Core.Scheduling;
using HA4IoT.Core.Settings;
using HA4IoT.Core.Timer;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Logger;
using HA4IoT.Networking;
using HA4IoT.Telemetry;

namespace HA4IoT.Core
{
    public class ControllerBase : IController
    {
        private readonly DeviceCollection _devices = new DeviceCollection();
        private readonly AreaCollection _areas = new AreaCollection();
        private readonly ComponentCollection _components = new ComponentCollection();
        private readonly AutomationCollection _automations = new AutomationCollection();
        private readonly SystemInformationService _systemInformationService = new SystemInformationService();
        private readonly int? _statusLedNumber;

        private BackgroundTaskDeferral _deferral;
        private HttpServer _httpServer;
        
        public IApiController ApiController { get; } = new ApiController("api");
        public IServiceLocator ServiceLocator { get; } = new ServiceLocator();
        public IHomeAutomationTimer Timer { get; protected set; }
        public ISettingsContainer Settings { get; private set; }

        protected ControllerBase(int? statusLedNumber)
        {
            _statusLedNumber = statusLedNumber;
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
        
        public void AddArea(IArea area)
        {
            _areas.AddUnique(area.Id, area);
        }

        public IArea GetArea(AreaId id)
        {
            return _areas.Get(id);
        }

        public IList<IArea> GetAreas()
        {
            return _areas.GetAll();
        }

        public void AddComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _components.AddUnique(component.Id, component);
        }

        public TComponent GetComponent<TComponent>() where TComponent : IComponent
        {
            return _components.Get<TComponent>();
        }

        public IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent
        {
            return _components.GetAll<TComponent>();
        }

        public IList<IComponent> GetComponents()
        {
            return _components.GetAll();
        }

        public bool GetContainsComponent(ComponentId componentId)
        {
            return _components.Contains(componentId);
        }

        public TComponent GetComponent<TComponent>(ComponentId id) where TComponent : IComponent
        {
            return _components.Get<TComponent>(id);
        }

        public void AddDevice(IDevice device)
        {
            _devices.AddUnique(device.Id, device);
        }

        public TDevice GetDevice<TDevice>(DeviceId id) where TDevice : IDevice
        {
            return _devices.Get<TDevice>(id);
        }

        public TDevice GetDevice<TDevice>() where TDevice : IDevice
        {
            return _devices.Get<TDevice>();
        }

        public IList<TDevice> GetDevices<TDevice>() where TDevice : IDevice
        {
            return _devices.GetAll<TDevice>();
        }

        public IList<IDevice> GetDevices()
        {
            return _devices.GetAll();
        }

        public void AddAutomation(IAutomation automation)
        {
            _automations.AddOrUpdate(automation.Id, automation);
        }

        public IList<TAutomation> GetAutomations<TAutomation>() where TAutomation : IAutomation
        {
            return _automations.GetAll<TAutomation>();
        }

        public TAutomation GetAutomation<TAutomation>(AutomationId id) where TAutomation : IAutomation
        {
            return _automations.Get<TAutomation>(id);
        }

        public IList<IAutomation> GetAutomations()
        {
            return _automations.GetAll();
        }

        protected virtual async Task ConfigureAsync()
        {
            await Task.FromResult(0);
        }

        private void InitializeHealthMonitor()
        {
            IBinaryOutput ledPin = null;
            if (_statusLedNumber.HasValue)
            {
                var pi2PortController = new Pi2PortController();
                ledPin = pi2PortController.GetOutput(_statusLedNumber.Value);
            }

            ServiceLocator.RegisterService(typeof(HealthService), new HealthService(ledPin, Timer, ServiceLocator.GetService<ISystemInformationService>()));
        }

        private void InitializeHttpApiEndpoint()
        {
            _httpServer = new HttpServer();

            var httpApiDispatcherEndpoint = new LocalHttpServerApiDispatcherEndpoint(_httpServer);
            ApiController.RegisterEndpoint(httpApiDispatcherEndpoint);

            var httpRequestDispatcher = new HttpRequestDispatcher(_httpServer);
            
            httpRequestDispatcher.MapFolder("App", StoragePath.AppRoot);
            httpRequestDispatcher.MapFolder("Storage", StoragePath.Root);
        }

        protected void InitializeAzureCloudApiEndpoint()
        {
            var azureCloudApiDispatcherEndpoint = new AzureCloudApiDispatcherEndpoint();

            if (azureCloudApiDispatcherEndpoint.TryInitializeFromConfigurationFile(
                StoragePath.WithFilename("AzureCloudApiDispatcherEndpointSettings.json")))
            {
                ApiController.RegisterEndpoint(azureCloudApiDispatcherEndpoint);
            }
        }

        private HomeAutomationTimer InitializeTimer()
        {
            var timer = new HomeAutomationTimer();
            Timer = timer;
            ServiceLocator.RegisterService(typeof(ISchedulerService), new SchedulerService(Timer));

            return timer;
        }

        private void InitializeLogging()
        {
            if (Log.Instance == null)
            {
                var udpLogger = new UdpLogger();
                udpLogger.Start();

                udpLogger.ExposeToApi(ApiController);

                Log.Instance = udpLogger;
            }
            
            Log.Info("Starting...");
        }

        private void InitializeCore()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                ServiceLocator.RegisterService(typeof(IDateTimeService), new DateTimeService());
                ServiceLocator.RegisterService(typeof(ISystemInformationService), _systemInformationService);

                InitializeLogging();
                InitializeHttpApiEndpoint();
                
                LoadControllerSettings();
                InitializeDiscovery();

                var timer = InitializeTimer();
                InitializeHealthMonitor();

                TryConfigure();
                CreateConfigurationStatistics();

                LoadNonControllerSettings();
                ResetActuatorStates();

                StartHttpServer();

                ExposeToApi();

                AttachComponentHistoryTracking();

                stopwatch.Stop();
                Log.Info("Startup completed after " + stopwatch.Elapsed);
                
                _systemInformationService.Set("Health/StartupDuration", stopwatch.Elapsed);
                _systemInformationService.Set("Health/StartupTimestamp", DateTime.Now);

                timer.Run();
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

        private void ResetActuatorStates()
        {
            foreach (var actuator in GetComponents<IActuator>())
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
                ConfigureAsync().Wait();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while configuring");
            }
        }

        private void LoadNonControllerSettings()
        {
            foreach (var area in _areas.GetAll())
            {
                area.Settings.Load();
            }

            foreach (var component in _components.GetAll())
            {
                component.Settings.Load();
            }

            foreach (var automation in _automations.GetAll())
            {
                automation.Settings.Load();
            }
        }

        private void ExposeToApi()
        {
            new ControllerApiDispatcher(this).ExposeToApi();
            
            foreach (var device in GetDevices())
            {
                ApiController.RouteRequest($"device/{device.Id}", device.HandleApiRequest);
                ApiController.RouteCommand($"device/{device.Id}", device.HandleApiCommand);
            }

            foreach (var area in _areas.GetAll())
            {
                new SettingsContainerApiDispatcher(area.Settings, $"area/{area.Id}", ApiController).ExposeToApi();
            }

            foreach (var component in _components.GetAll())
            {
                new SettingsContainerApiDispatcher(component.Settings, $"component/{component.Id}", ApiController).ExposeToApi();
                ApiController.RouteCommand($"component/{component.Id}/status", component.HandleApiCommand);
                ApiController.RouteRequest($"component/{component.Id}/status", component.HandleApiRequest);
                component.StateChanged += (s, e) => ApiController.NotifyStateChanged(component);
            }

            foreach (var automation in _automations.GetAll())
            {
                new SettingsContainerApiDispatcher(automation.Settings, $"automation/{automation.Id}", ApiController).ExposeToApi();
            }
        }

        private void AttachComponentHistoryTracking()
        {
            foreach (var component in GetComponents())
            {
                var history = new ComponentStateHistoryTracker(component);
                history.ExposeToApi(ApiController);
            }
        }

        private void CreateConfigurationStatistics()
        {
            var systemInformationService = ServiceLocator.GetService<ISystemInformationService>();
            systemInformationService.Set("Components/Count", _components.GetAll().Count);
            systemInformationService.Set("Areas/Count", _areas.GetAll().Count);
            systemInformationService.Set("Automations/Count", _automations.GetAll().Count);

            systemInformationService.Set("Services/Count", ServiceLocator.GetServices().Count);
        }
    }
}
