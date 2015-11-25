using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Core;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Home : IStatusProvider
    {
        private readonly HealthMonitor _healthMonitor;
        private readonly Dictionary<Enum, Room> _rooms = new Dictionary<Enum, Room>();
        private readonly HashAlgorithmProvider _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);

        public Home(IHomeAutomationTimer timer, HealthMonitor healthMonitor, IWeatherStation weatherStation, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));
            if (notificationHandler == null) throw new ArgumentNullException(nameof(notificationHandler));

            Timer = timer;
            _healthMonitor = healthMonitor;
            WeatherStation = weatherStation;
            HttpApiController = httpApiController;
            NotificationHandler = notificationHandler;

            httpApiController.Handle(HttpMethod.Get, "configuration").Using(c => c.Response.Body = new JsonBody(GetConfigurationAsJson()));
            httpApiController.Handle(HttpMethod.Get, "status").Using(HandleStatusRequest);
            httpApiController.Handle(HttpMethod.Get, "health").Using(c => c.Response.Body = new JsonBody(_healthMonitor.ApiGet()));
        }

        public IHomeAutomationTimer Timer { get; }

        public IHttpRequestController HttpApiController { get; }

        public INotificationHandler NotificationHandler { get; }

        public IWeatherStation WeatherStation { get; }

        public Dictionary<Enum, ActuatorBase> Actuators { get; } = new Dictionary<Enum, ActuatorBase>();

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
            NotificationHandler.Info("Registered actuators = {0}, Rooms = {1}.", Actuators.Count, _rooms.Count);
        }

        private void HandleStatusRequest(HttpContext httpContext)
        {
            var currentStatus = GetStatus().Stringify();
            var currentHash = "\"" + GenerateHash(currentStatus) + "\"";

            string clientHash;
            if (httpContext.Request.Headers.TryGetValue("If-None-Match", out clientHash))
            {
                if (clientHash.Equals(currentHash))
                {
                    httpContext.Response.StatusCode = HttpStatusCode.NotModified;
                    return;
                }
            }

            httpContext.Response.StatusCode = HttpStatusCode.OK;
            httpContext.Response.Headers.Add("ETag", currentHash);

            // Prevent the JsonObject from performing two redundant "Stringify" calls by using the plain text body with JSON mime type.
            httpContext.Response.Body = new PlainTextBody().WithContent(currentStatus).WithMimeType(MimeTypeProvider.Json);
        }

        public JsonObject GetStatus()
        {
            var result = new JsonObject();
            result.SetNamedValue("type", JsonValue.CreateStringValue("HA4IoT.Status"));
            result.SetNamedValue("version", JsonValue.CreateNumberValue(1));
            
            var status = new JsonObject();
            foreach (var actuator in Actuators.Values)
            {
                var context = new ApiRequestContext(new JsonObject(), new JsonObject());
                actuator.HandleApiGet(context);

                status.SetNamedValue(actuator.Id, context.Response);
            }

            result.SetNamedValue("status", status);

            if (WeatherStation != null)
            {
                result.SetNamedValue("weatherStation", WeatherStation.GetStatus());
            }

            return result;
        }

        private JsonObject GetConfigurationAsJson()
        {
            var configuration = new JsonObject();
            configuration.SetNamedValue("type", JsonValue.CreateStringValue("HA4IoT.Configuration"));
            configuration.SetNamedValue("version", JsonValue.CreateNumberValue(1));

            var rooms = new JsonObject();
            foreach (var room in _rooms.Values)
            {
                rooms.SetNamedValue(room.Id, room.GetConfigurationAsJson());
            }

            configuration.SetNamedValue("rooms", rooms);

            return configuration;
        }

        private string GenerateHash(string input)
        {
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            IBuffer hashBuffer = _hashAlgorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToBase64String(hashBuffer);
        }
    }
}