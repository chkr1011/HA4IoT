using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using HA4IoT.Core.Discovery;
using HA4IoT.Core.Settings;
using HA4IoT.Core.Timer;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Logger;
using HA4IoT.Networking;
using HA4IoT.Telemetry;

namespace HA4IoT.Core
{
    public abstract class ControllerBase : IController
    {
        private readonly DeviceCollection _devices = new DeviceCollection();
        private readonly AreaCollection _areas = new AreaCollection();
        private readonly ComponentCollection _components = new ComponentCollection();
        private readonly AutomationCollection _automations = new AutomationCollection();
        private readonly List<IService> _services = new List<IService>(); 

        private HealthMonitor _healthMonitor;
        private BackgroundTaskDeferral _deferral;
        private HttpServer _httpServer;

        public IApiController ApiController { get; } = new ApiController("api"); 
        public IHomeAutomationTimer Timer { get; protected set; }
        public ISettingsContainer Settings { get; private set; }

        public Task RunAsync(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null) throw new ArgumentNullException(nameof(taskInstance));

            _deferral = taskInstance.GetDeferral();

            return RunAsync();
        }

        public Task RunAsync()
        {
            return Task.Factory.StartNew(
                InitializeCore,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
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

        public void RegisterService<TService>(TService service) where TService : IService
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            TService tmp;
            if (TryGetService(out tmp))
            {
                throw new InvalidOperationException($"Service {service.GetType().FullName} is already registered.");
            }

            _services.Add(service);
        }

        public TService GetService<TService>() where TService : IService
        {
            TService service;
            if (!TryGetService(out service))
            {
                throw new InvalidOperationException($"Service {typeof(TService).FullName} is not registered.");
            }

            return service;
        }

        public IList<IService> GetServices()
        {
            return _services;
        }

        public bool TryGetService<TService>(out TService service) where TService : IService
        {
            service = _services.OfType<TService>().FirstOrDefault();
            if (service == null)
            {
                return false;
            }

            return true;
        }

        protected virtual async Task InitializeAsync()
        {
            await Task.FromResult(0);
        }

        protected void InitializeHealthMonitor(int pi2GpioPinWithLed)
        {
            var pi2PortController = new Pi2PortController();
            var ledPin = pi2PortController.GetOutput(pi2GpioPinWithLed);

            _healthMonitor = new HealthMonitor(ledPin, Timer, ApiController);
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

            azureCloudApiDispatcherEndpoint.TryInitializeFromConfigurationFile(
                StoragePath.WithFilename("AzureCloudApiDispatcherEndpointSettings.json"));

            ApiController.RegisterEndpoint(azureCloudApiDispatcherEndpoint);
        }

        private HomeAutomationTimer InitializeTimer()
        {
            var timer = new HomeAutomationTimer();
            Timer = timer;

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
            var stopwatch = Stopwatch.StartNew();

            try
            {
                InitializeLogging();
                InitializeHttpApiEndpoint();
                
                LoadControllerSettings();
                InitializeDiscovery();

                HomeAutomationTimer timer = InitializeTimer();

                TryInitialize();
                
                LoadNonControllerSettings();
                ResetActuatorStates();

                _httpServer.Start(80);
                ExposeToApi();

                AttachComponentHistoryTracking();

                stopwatch.Stop();
                Log.Info("Startup completed after " + stopwatch.Elapsed);

                timer.Run();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to initialize.");
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
                    Log.Warning(exception, "Error while initially reset of state for actuator {actuator.Id}.");
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

        private void TryInitialize()
        {
            try
            {
                InitializeAsync().Wait();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while initializing");
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

            foreach (var service in GetServices())
            {
                ApiController.RouteRequest($"service/{service.GetType().Name}", service.HandleApiRequest);
                ApiController.RouteCommand($"service/{service.GetType().Name}", service.HandleApiCommand);
            }

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
    }
}
