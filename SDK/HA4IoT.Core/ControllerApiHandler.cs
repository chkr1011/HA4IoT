using System;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Networking;

namespace HA4IoT.Core
{
    public class ControllerApiHandler
    {
        private readonly HashAlgorithmProvider _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
        private readonly IController _controller;
        
        public ControllerApiHandler(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }

        public void ExposeToApi()
        {
            _controller.HttpApiController.Handle(HttpMethod.Get, "configuration").Using(c => c.Response.Body = new JsonBody(GetConfigurationAsJson()));
            _controller.HttpApiController.Handle(HttpMethod.Get, "status").Using(HandleStatusRequest);
        }

        private JsonObject GetStatus()
        {
            var result = new JsonObject();
            result.SetNamedValue("type", JsonValue.CreateStringValue("HA4IoT.Status"));
            result.SetNamedValue("version", JsonValue.CreateNumberValue(1));

            var status = new JsonObject();
            foreach (var actuator in _controller.GetActuators())
            {
                status.SetNamedValue(actuator.Id.Value, actuator.GetStatusForApi());
            }

            result.SetNamedValue("status", status);

            if (_controller.WeatherStation != null)
            {
                result.SetNamedValue("weatherStation", _controller.WeatherStation.GetStatusForApi());
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
            foreach (var room in _controller.GetRooms())
            {
                rooms.SetNamedValue(room.Id.Value, GetRoomConfigurationAsJson(room));
            }

            configuration.SetNamedValue("rooms", rooms);

            return configuration;
        }

        private JsonObject GetRoomConfigurationAsJson(IRoom room)
        {
            var actuators = new JsonObject();
            foreach (var actuator in room.GetActuators())
            {
                actuators.SetNamedValue(actuator.Id.Value, actuator.GetConfigurationForApi());
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
    }
}
