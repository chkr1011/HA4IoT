using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.Storage;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Core
{
    public abstract class ControllerBase : IController
    {
        private readonly RoomCollection _rooms = new RoomCollection();
        private readonly ActuatorCollection _actuators = new ActuatorCollection();
        private readonly DeviceCollection _devices = new DeviceCollection();

        private HealthMonitor _healthMonitor;
        private BackgroundTaskDeferral _deferral;
        private HttpServer _httpServer;

        public INotificationHandler Logger { get; protected set; }
        public IHttpRequestController HttpApiController { get; protected set; }
        public IHomeAutomationTimer Timer { get; protected set; }
        public IWeatherStation WeatherStation { get; protected set; }
        
        public void RunAsync(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null) throw new ArgumentNullException(nameof(taskInstance));

            _deferral = taskInstance.GetDeferral();
            Task.Factory.StartNew(InitializeCore, TaskCreationOptions.LongRunning);
        }
        
        public void AddRoom(IRoom room)
        {
            _rooms.Add(room);
        }

        public IRoom Room(RoomId id)
        {
            return _rooms[id];
        }

        public IList<IRoom> Rooms()
        {
            return _rooms.GetAll();
        }

        public void AddActuator(IActuator actuator)
        {
            _actuators.Add(actuator);
        }

        public IList<IActuator> Actuators()
        {
            return _actuators.GetAll();
        }

        public void AddDevice(IDevice device)
        {
            _devices.Add(device);
        }

        public TDevice Device<TDevice>(Enum id) where TDevice : IDevice
        {
            return Device<TDevice>(new DeviceId(id.ToString()));
        }

        public TDevice Device<TDevice>(DeviceId id) where TDevice : IDevice
        {
            return _devices.Get<TDevice>(id);
        }

        public TDevice Device<TDevice>() where TDevice : IDevice
        {
            return _devices.Get<TDevice>();
        }

        public IList<TDevice> Devices<TDevice>() where TDevice : IDevice
        {
            return _devices.GetAll<TDevice>();
        }

        protected void PublishStatisticsNotification()
        {
            int totalActuatorsCount = 0;

            var message = new StringBuilder();
            message.AppendLine("Controller statistics after initialization:");
            message.AppendFormat("- Devices={0}", Devices<IDevice>().Count);
            message.AppendLine();

            foreach (var room in Rooms())
            {
                var actuatorsCount = room.Actuators().Count;
                totalActuatorsCount += actuatorsCount;

                message.AppendFormat("- Room '{0}', Actuators={1}", room.Id, actuatorsCount);
                message.AppendLine();
            }

            message.AppendLine("Total actuators=" + totalActuatorsCount);

            Logger.Info(message.ToString());
        }

        protected virtual void Initialize()
        {
        }

        protected void InitializeHealthMonitor(int pi2GpioPinWithLed)
        {
            var pi2PortController = GpioController.GetDefault();
            var ledPin = pi2PortController.OpenPin(pi2GpioPinWithLed);
            ledPin.SetDriveMode(GpioPinDriveMode.Output);

            _healthMonitor = new HealthMonitor(ledPin, Timer, HttpApiController);
        }

        private void InitializeHttpApi()
        {
            _httpServer = new HttpServer();
            var httpRequestDispatcher = new HttpRequestDispatcher(_httpServer);
            HttpApiController = httpRequestDispatcher.GetController("api");

            var appPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "app");
            httpRequestDispatcher.MapFolder("app", appPath);
        }

        protected void InitializeWeatherStation(IWeatherStation weatherStation)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));

            WeatherStation = weatherStation;
        }

        private void InitializeTimer()
        {
            Timer = new HomeAutomationTimer(Logger);
        }

        private void InitializeNotificationHandler()
        {
            var logger = new NotificationHandler();
            logger.ExposeToApi(HttpApiController);
            logger.Info("Starting");
            Logger = logger;
        }

        private void InitializeCore()
        {
            InitializeHttpApi();

            InitializeNotificationHandler();
            InitializeTimer();

            var controllerApiHandler = new ControllerApiHandler(this);
            controllerApiHandler.ExposeToApi();

            TryInitializeActuators();

            _httpServer.StartAsync(80).Wait();
            Timer.Run();
        }

        private void TryInitializeActuators()
        {
            try
            {
                Initialize();
            }
            catch (Exception exception)
            {
                Logger.Error("Error while initializing actuators. " + exception);
            }
        }
    }
}
