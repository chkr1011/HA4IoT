using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using HA4IoT.Api;
using HA4IoT.Api.AzureCloud;
using HA4IoT.Api.LocalHttpServer;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Core.Discovery;
using HA4IoT.Core.Timer;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Networking;
using HA4IoT.Telemetry.History;

namespace HA4IoT.Core
{
    public abstract class ControllerBase : IController
    {
        private readonly DeviceCollection _devices = new DeviceCollection();
        private readonly AreaCollection _areas = new AreaCollection();
        private readonly ActuatorCollection _actuators = new ActuatorCollection();
        private readonly AutomationCollection _automations = new AutomationCollection();
        private readonly List<IService> _services = new List<IService>(); 

        private HealthMonitor _healthMonitor;
        private BackgroundTaskDeferral _deferral;
        private HttpServer _httpServer;

        public IApiController ApiController { get; } = new ApiController("api"); 
        public IHomeAutomationTimer Timer { get; protected set; }
        public IControllerSettings Settings { get; private set; }

        public void RunAsync(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null) throw new ArgumentNullException(nameof(taskInstance));

            _deferral = taskInstance.GetDeferral();

            Task.Factory.StartNew(
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

        public void AddActuator(IActuator actuator)
        {
            _actuators.AddOrUpdate(actuator.Id, actuator);
        }

        public TActuator GetActuator<TActuator>(ActuatorId id) where TActuator : IActuator
        {
            return _actuators.Get<TActuator>(id);
        }

        public TActuator GetActuator<TActuator>() where TActuator : IActuator
        {
            return _actuators.Get<TActuator>();
        }

        public IList<TActuator> GetActuators<TActuator>() where TActuator : IActuator
        {
            return _actuators.GetAll<TActuator>();
        }

        public IList<IActuator> GetActuators()
        {
            return _actuators.GetAll();
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

        public void RegisterService(IService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            IService tmp;
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

        public bool TryGetService<TService>(out TService service) where TService : IService
        {
            service = _services.OfType<TService>().FirstOrDefault();
            if (service == null)
            {
                return false;
            }

            return true;
        }

        protected virtual void Initialize()
        {
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
            
            httpRequestDispatcher.MapFolder("App", StoragePath.WithFilename("App"));
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
            var logger = new Logger.Logger();
            Log.Instance = logger;

            logger.ExposeToApi(ApiController);
            logger.Info("Starting...");
        }

        private void InitializeCore()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                InitializeHttpApiEndpoint();
                InitializeLogging();
                LoadControllerSettings();
                InitializeDiscovery();

                HomeAutomationTimer timer = InitializeTimer();

                TryInitialize();
                LoadNonControllerSettings();

                _httpServer.Start(80);
                ExposeToApi();

                AttachActuatorHistory();

                stopwatch.Stop();
                Log.Info("Startup completed after " + stopwatch.Elapsed);

                timer.Run();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
        }

        private void InitializeDiscovery()
        {
            var discoveryServer = new DiscoveryServer(this);
            discoveryServer.Start();
        }

        private void LoadControllerSettings()
        {
            var settings = new ControllerSettings(StoragePath.WithFilename("Settings.json"));
            settings.Load();

            Settings = settings;
        }

        private void TryInitialize()
        {
            try
            {
                Initialize();
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
                area.LoadSettings();
            }

            foreach (var actuator in _actuators.GetAll())
            {
                actuator.LoadSettings();
            }

            foreach (var automation in _automations.GetAll())
            {
                automation.LoadSettings();
            }
        }

        private void ExposeToApi()
        {
            new ControllerApiDispatcher(this).ExposeToApi();

            foreach (var area in _areas.GetAll())
            {
                area.ExposeToApi();
            }

            foreach (var actuator in _actuators.GetAll())
            {
                actuator.ExposeToApi();
            }
        }

        private void AttachActuatorHistory()
        {
            foreach (IActuator actuator in GetActuators())
            {
                var sensorActuator = actuator as ISingleValueSensorActuator;
                if (sensorActuator != null)
                {
                    var history = new SensorActuatorHistory(sensorActuator);
                    history.ExposeToApi(ApiController);
                }
            }
        }
    }
}
