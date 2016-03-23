using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.WeatherService;
using HA4IoT.Networking;

namespace HA4IoT.Core
{
    public class ControllerApiDispatcher
    {
        private readonly IController _controller;

        public ControllerApiDispatcher(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            _controller = controller;
        }

        public void ExposeToApi()
        {
            _controller.ApiController.RouteRequest("configuration", HandleApiGetConfiguration);
            _controller.ApiController.RouteRequest("status", HandleApiGetStatus);
        }

        private void HandleApiGetStatus(IApiContext apiContext)
        {
            var result = new JsonObject();
            result.SetNamedString("Type", "HA4IoT.Status");
            result.SetNamedNumber("Version", 1D);

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

            var weatherStation = _controller.GetService<IWeatherService>();
            if (weatherStation != null)
            {
                result.SetNamedValue("WeatherStation", weatherStation.ExportStatusToJsonObject());
            }

            apiContext.Response = result;
        }

        private void HandleApiGetConfiguration(IApiContext apiContext)
        {
            var configuration = new JsonObject();
            configuration.SetNamedValue("Type", JsonValue.CreateStringValue("HA4IoT.Configuration"));
            configuration.SetNamedValue("Version", JsonValue.CreateNumberValue(1));

            var areas = new JsonObject();
            foreach (var area in _controller.GetAreas())
            {
                areas.SetNamedValue(area.Id.Value, ExportAreaConfigurationToJsonValue(area));
            }

            configuration.SetNamedValue("Areas", areas);

            apiContext.Response = configuration;
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
    }
}
