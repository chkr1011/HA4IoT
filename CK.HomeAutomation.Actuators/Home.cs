using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using CK.HomeAutomation.Core;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class Home
    {
        private readonly Dictionary<Enum, Room> _rooms = new Dictionary<Enum, Room>();
        private readonly HealthMonitor _healthMonitor;

        public Home(HomeAutomationTimer timer, HealthMonitor healthMonitor, IWeatherStation weatherStation, HttpRequestController httpApiController, INotificationHandler notificationHandler)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            Timer = timer;
            _healthMonitor = healthMonitor;
            WeatherStation = weatherStation;
            HttpApiController = httpApiController;
            NotificationHandler = notificationHandler;

            httpApiController.Handle(HttpMethod.Get, "configuration").Using(c => c.Response.Result = GetConfigurationAsJSON());
            httpApiController.Handle(HttpMethod.Get, "status").Using(c => c.Response.Result = GetStatusAsJSON());
        }

        public HomeAutomationTimer Timer { get; }

        public HttpRequestController HttpApiController { get; }

        public INotificationHandler NotificationHandler { get; }

        public IWeatherStation WeatherStation { get; }

        public Dictionary<Enum, BaseActuator> Actuators { get; } = new Dictionary<Enum, BaseActuator>();

        public Room AddRoom(Enum id)
        {
            var room = new Room(this, id.ToString());
            _rooms.Add(id, room);

            return room;
        }

        public Room Room(Enum id)
        {
            return _rooms[id];
        }

        public void PublishStatisticsNotification()
        {
            NotificationHandler.PublishFrom(this, NotificationType.Info, "Registered actuators = {0}, Rooms = {1}.", Actuators.Count, _rooms.Count);
        }

        private JsonObject GetStatusAsJSON()
        {
            var result = new JsonObject();
            foreach (var actuator in Actuators.Values)
            {
                var context = new ApiRequestContext(new JsonObject(), new JsonObject());
                actuator.ApiGet(context);

                result.SetNamedValue(actuator.Id, context.Response);
            }

            if (WeatherStation != null)
            {
                result.SetNamedValue("weatherStation", WeatherStation.GetStatusAsJSON());
            }

            if (_healthMonitor != null)
            {
                result.SetNamedValue("health", _healthMonitor.GetStatusAsJSON());
            }

            string jsonText = result.Stringify();
            result.SetNamedValue("_hash", JsonValue.CreateStringValue(GenerateHash(jsonText)));

            return result;
        }

        private JsonObject GetConfigurationAsJSON()
        {
            JsonObject state = new JsonObject();
            foreach (var room in _rooms.Values)
            {
                JsonObject roomConfiguration = room.GetConfigurationAsJSON();
                state.SetNamedValue(room.Id, roomConfiguration);
            }

            return state;
        }

        private string GenerateHash(string input)
        {
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            IBuffer hashBuffer = hashAlgorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToBase64String(hashBuffer);
        }
    }
}