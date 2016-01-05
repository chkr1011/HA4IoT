using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Devices.Gpio;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Core
{
    public abstract class ControllerBase : IController
    {
        private readonly Dictionary<RoomId, IRoom> _rooms = new Dictionary<RoomId, IRoom>();
        private readonly HashAlgorithmProvider _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);

        private HealthMonitor _healthMonitor;
        private BackgroundTaskDeferral _deferral;
        private HttpServer _httpServer;

        public INotificationHandler Logger { get; private set; }
        public IHttpRequestController HttpApiController { get; private set; }
        public IHomeAutomationTimer Timer { get; private set; }
        public IWeatherStation WeatherStation { get; set; }

        public Dictionary<ActuatorId, IActuator> Actuators { get; } = new Dictionary<ActuatorId, IActuator>();

        public void RunAsync(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            Task.Factory.StartNew(InitializeCore, TaskCreationOptions.LongRunning);
        }

        public IRoom Room(RoomId id)
        {
            IRoom room;
            if (!_rooms.TryGetValue(id, out room))
            {
                throw new InvalidOperationException("Room with ID '" + id + "' is not registered.");
            }

            return room;
        }

        public IRoom CreateRoom(Enum id)
        {
            var roomId = new RoomId(id.ToString());

            if (_rooms.ContainsKey(roomId))
            {
                throw new InvalidOperationException("Room with ID '" + roomId + "' aready registered.");
            }
            
            // TODO: use RoomIdFactory.
            var room = new Room(roomId, this);
            AddRoom(room);

            return room;
        }

        public void AddRoom(IRoom room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            if (_rooms.ContainsKey(room.Id))
            {
                throw new InvalidOperationException("Room with ID '" + room.Id + "' aready registered.");
            }

            _rooms.Add(room.Id, room);
        }

        protected void PublishStatisticsNotification()
        {
            Logger.Info("Registered actuators = {0}, Rooms = {1}.", Actuators.Count, _rooms.Count);
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

        private JsonObject GetStatus()
        {
            var result = new JsonObject();
            result.SetNamedValue("type", JsonValue.CreateStringValue("HA4IoT.Status"));
            result.SetNamedValue("version", JsonValue.CreateNumberValue(1));

            var status = new JsonObject();
            foreach (var actuator in Actuators.Values)
            {
                status.SetNamedValue(actuator.Id.Value, actuator.GetStatusForApi());
            }

            result.SetNamedValue("status", status);

            if (WeatherStation != null)
            {
                result.SetNamedValue("weatherStation", WeatherStation.GetStatusForApi());
            }

            return result;
        }

        private void HandleStatusRequest(HttpContext httpContext)
        {
            var status = GetStatus();
            var hash = GenerateHash(status.Stringify());
            var hashWithQuotes = "\"" + hash + "\"";

            string clientHash;
            if (httpContext.Request.Headers.TryGetValue("If-None-Match", out clientHash))
            {
                if (clientHash.Equals(hashWithQuotes))
                {
                    httpContext.Response.StatusCode = HttpStatusCode.NotModified;
                    return;
                }
            }

            status.SetNamedValue("hash", hash.ToJsonValue());

            httpContext.Response.StatusCode = HttpStatusCode.OK;
            httpContext.Response.Headers.Add("ETag", hashWithQuotes);

            httpContext.Response.Body = new JsonBody(status);
        }

        private JsonObject GetConfigurationAsJson()
        {
            var configuration = new JsonObject();
            configuration.SetNamedValue("type", JsonValue.CreateStringValue("HA4IoT.Configuration"));
            configuration.SetNamedValue("version", JsonValue.CreateNumberValue(1));

            var rooms = new JsonObject();
            foreach (var room in _rooms.Values)
            {
                rooms.SetNamedValue(room.Id.Value, GetRoomConfigurationAsJson(room));
            }

            configuration.SetNamedValue("rooms", rooms);

            return configuration;
        }


        private JsonObject GetRoomConfigurationAsJson(IRoom room)
        {
            var actuators = new JsonObject();
            foreach (var actuator in room.Actuators)
            {
                actuators.SetNamedValue(actuator.Key.Value, actuator.Value.GetConfigurationForApi());
            }

            JsonObject configuration = new JsonObject();
            configuration.SetNamedValue("actuators", actuators);
            return configuration;
        }

        private string GenerateHash(string input)
        {
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            IBuffer hashBuffer = _hashAlgorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToBase64String(hashBuffer);
        }

        private void InitializeHttpApi()
        {
            _httpServer = new HttpServer();
            var httpRequestDispatcher = new HttpRequestDispatcher(_httpServer);
            HttpApiController = httpRequestDispatcher.GetController("api");

            var appPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "app");
            httpRequestDispatcher.MapFolder("app", appPath);
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

            HttpApiController.Handle(HttpMethod.Get, "configuration").Using(c => c.Response.Body = new JsonBody(GetConfigurationAsJson()));
            HttpApiController.Handle(HttpMethod.Get, "status").Using(HandleStatusRequest);
            HttpApiController.Handle(HttpMethod.Get, "health").Using(c => c.Response.Body = new JsonBody(_healthMonitor.ApiGet()));

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
