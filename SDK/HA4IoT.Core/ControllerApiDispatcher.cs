using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Core;
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
            result.SetNamedString("type", "HA4IoT.Status");
            result.SetNamedNumber("version", 1D);

            var services = new JsonObject();
            foreach (var service in _controller.GetServices())
            {
                services.SetNamedObject(service.GetType().Name, service.ExportStatusToJsonObject());
            }

            result.SetNamedValue("services", services);

            var components = new JsonObject();
            foreach (var component in _controller.GetComponents())
            {
                components.SetNamedValue(component.Id.Value, component.ExportStatusToJsonObject());
            }

            result.SetNamedValue("components", components);

            var automations = new JsonObject();
            foreach (var automation in _controller.GetAutomations())
            {
                automations.SetNamedValue(automation.Id.Value, automation.ExportStatusToJsonObject());
            }
             
            result.SetNamedValue("automations", automations);

            apiContext.Response = result;
        }

        private void HandleApiGetConfiguration(IApiContext apiContext)
        {
            var configuration = new JsonObject();
            configuration.SetNamedString("type", "HA4IoT.Configuration");
            configuration.SetNamedNumber("version", 1D);

            var areas = new JsonObject();
            foreach (var area in _controller.GetAreas())
            {
                areas.SetNamedValue(area.Id.Value, ExportAreaConfigurationToJsonValue(area));
            }

            configuration.SetNamedValue("areas", areas);

            apiContext.Response = configuration;
        }

        private IJsonValue ExportAreaConfigurationToJsonValue(IArea area)
        {
            var configuration = new JsonObject();
            configuration.SetNamedValue("settings", area.ExportConfigurationToJsonObject());

            var components = new JsonObject();
            foreach (var component in area.GetComponents())
            {
                components.SetNamedValue(component.Id.Value, component.ExportConfigurationToJsonObject());
            }

            configuration.SetNamedValue("components", components);

            var automations = new JsonObject();
            foreach (var automation in area.GetAutomations())
            {
                automations.SetNamedValue(automation.Id.Value, automation.ExportConfigurationAsJsonValue());
            }

            configuration.SetNamedValue("automations", automations);

            return configuration;
        }
    }
}
