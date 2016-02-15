using System;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.WeatherStation;
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
            result.SetNamedValue("type", "HA4IoT.Status".ToJsonValue());
            result.SetNamedValue("version", 1.ToJsonValue());

            var status = new JsonObject();
            foreach (var actuator in _controller.Actuators())
            {
                status.SetNamedValue(actuator.Id.Value, actuator.GetStatusForApi());
            }

            result.SetNamedValue("status", status);

            var weatherStation = _controller.Device<IWeatherStation>();
            if (weatherStation != null)
            {
                result.SetNamedValue("weatherStation", weatherStation.GetStatusForApi());
            }

            return result;
        }

        private void HandleStatusRequest(HttpContext httpContext)
        {
            var status = GetStatus();
            var hash = GenerateHash(status.Stringify());
            var hashWithQuotes = "\"" + hash + "\"";

            string clientHash;
            if (httpContext.Request.Headers.TryGetValue(HttpHeaderNames.IfNoneMatch, out clientHash))
            {
                if (clientHash.Equals(hashWithQuotes))
                {
                    httpContext.Response.StatusCode = HttpStatusCode.NotModified;
                    return;
                }
            }

            status.SetNamedValue("_hash", hash.ToJsonValue());

            httpContext.Response.StatusCode = HttpStatusCode.OK;
            httpContext.Response.Headers[HttpHeaderNames.ETag] = hashWithQuotes;

            httpContext.Response.Body = new JsonBody(status);
        }

        private JsonObject GetConfigurationAsJson()
        {
            var configuration = new JsonObject();
            configuration.SetNamedValue("type", JsonValue.CreateStringValue("HA4IoT.Configuration"));
            configuration.SetNamedValue("version", JsonValue.CreateNumberValue(1));

            var rooms = new JsonObject();
            foreach (var room in _controller.Areas())
            {
                rooms.SetNamedValue(room.Id.Value, GetAreaConfigurationAsJson(room));
            }

            configuration.SetNamedValue("rooms", rooms);

            return configuration;
        }

        private JsonObject GetAreaConfigurationAsJson(IArea area)
        {
            var actuators = new JsonObject();
            foreach (var actuator in area.Actuators())
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
