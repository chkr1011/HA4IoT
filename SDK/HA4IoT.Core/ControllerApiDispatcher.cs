using System;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Networking;

namespace HA4IoT.Core
{
    public class ControllerApiDispatcher
    {
        private readonly HashAlgorithmProvider _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
        private readonly IController _controller;

        public ControllerApiDispatcher(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }

        public void ExposeToApi()
        {
            _controller.HttpApiController.HandleGet("configuration").Using(HandleApiGetConfiguration);
            _controller.HttpApiController.HandleGet("status").Using(HandleApiGetStatus);
        }

        private void HandleApiGetStatus(HttpContext httpContext)
        {
            var stopwatch = Stopwatch.StartNew();

            var status = GetControllerStatus();
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

            status.SetNamedValue("Hash", hash.ToJsonValue());
            status.SetNamedValue("GenerationDuration", stopwatch.Elapsed.ToJsonValue());

            httpContext.Response.StatusCode = HttpStatusCode.OK;
            httpContext.Response.Headers[HttpHeaderNames.ETag] = hashWithQuotes;

            httpContext.Response.Body = new JsonBody(status);
        }

        private void HandleApiGetConfiguration(HttpContext httpContex)
        {
            var stopwatch = Stopwatch.StartNew();

            var configuration = new JsonObject();
            configuration.SetNamedValue("Type", JsonValue.CreateStringValue("HA4IoT.Configuration"));
            configuration.SetNamedValue("Version", JsonValue.CreateNumberValue(1));

            var areas = new JsonObject();
            foreach (var area in _controller.GetAreas())
            {
                areas.SetNamedValue(area.Id.Value, ExportAreaConfigurationToJsonValue(area));
            }

            configuration.SetNamedValue("Areas", areas);
            configuration.SetNamedValue("GenerationDuration", stopwatch.Elapsed.ToJsonValue());

            httpContex.Response.Body = new JsonBody(configuration);
        }

        private IJsonValue ExportAreaConfigurationToJsonValue(IArea area)
        {
            var configuration = new JsonObject();
            configuration.SetNamedValue("Settings", area.ExportConfigurationToJsonObject());

            var actuators = new JsonObject();
            foreach (var actuator in area.GetActuators())
            {
                actuators.SetNamedValue(actuator.Id.Value, actuator.ExportConfigurationToJsonObject());
            }

            configuration.SetNamedValue("Actuators", actuators);

            var automations = new JsonObject();
            foreach (var automation in area.GetAutomations())
            {
                automations.SetNamedValue(automation.Id.Value, automation.ExportConfigurationAsJsonValue());
            }

            configuration.SetNamedValue("Automations", automations);

            return configuration;
        }

        private JsonObject GetControllerStatus()
        {
            var result = new JsonObject();
            result.SetNamedValue("Type", "HA4IoT.Status".ToJsonValue());
            result.SetNamedValue("Version", 1.ToJsonValue());

            var actuators = new JsonObject();
            foreach (var actuator in _controller.GetActuators())
            {
                actuators.SetNamedValue(actuator.Id.Value, actuator.ExportStatusToJsonObject());
            }

            result.SetNamedValue("Actuators", actuators);

            var automations = new JsonObject();
            foreach (var automation in _controller.GetAutomations())
            {
                automations.SetNamedValue(automation.Id.Value, automation.ExportStatusToJsonObject());
            }

            result.SetNamedValue("Automations", automations);

            var weatherStation = _controller.GetDevice<IWeatherStation>();
            if (weatherStation != null)
            {
                result.SetNamedValue("WeatherStation", weatherStation.ExportStatusToJsonObject());
            }

            return result;
        }

        private string GenerateHash(string input)
        {
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            IBuffer hashBuffer = _hashAlgorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToBase64String(hashBuffer);
        }
    }
}
