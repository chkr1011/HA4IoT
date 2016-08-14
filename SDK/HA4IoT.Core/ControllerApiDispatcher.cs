using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Core
{
    public class ControllerApiDispatcher
    {
        private readonly IApiService _apiService;
        private readonly IDeviceService _deviceService;
        private readonly IComponentService _componentService;
        private readonly IAreaService _areaService;
        private readonly IAutomationService _automationService;

        public ControllerApiDispatcher(
            IApiService apiService,
            IDeviceService deviceService,
            IComponentService componentService,
            IAreaService areaService,
            IAutomationService automationService)
        {
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (deviceService == null) throw new ArgumentNullException(nameof(deviceService));
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (automationService == null) throw new ArgumentNullException(nameof(automationService));

            _apiService = apiService;
            _deviceService = deviceService;
            _componentService = componentService;
            _areaService = areaService;
            _automationService = automationService;
        }

        public void ExposeToApi()
        {
            _apiService.RouteRequest("configuration", HandleApiGetConfiguration);
            _apiService.RouteRequest("status", HandleApiGetStatus);

            ExposeServicesToApi();
        }

        private void ExposeServicesToApi()
        {
            foreach (var service in _controller.ServiceLocator.GetServices())
            {
                _apiService.RouteRequest($"service/{service.InterfaceType.Name}", service.ServiceInstance.HandleApiRequest);
                _apiService.RouteCommand($"service/{service.InterfaceType.Name}", service.ServiceInstance.HandleApiCommand);
            }

            foreach (var device in _deviceService.GetDevices())
            {
                _apiService.RouteRequest($"device/{device.Id}", device.HandleApiRequest);
                _apiService.RouteCommand($"device/{device.Id}", device.HandleApiCommand);
            }

            foreach (var area in _areaService.GetAreas())
            {
                new SettingsContainerApiDispatcher(area.Settings, $"area/{area.Id}", _apiService).ExposeToApi();
            }

            foreach (var component in _componentService.GetComponents())
            {
                new SettingsContainerApiDispatcher(component.Settings, $"component/{component.Id}", _apiService).ExposeToApi();
                _apiService.RouteCommand($"component/{component.Id}/status", component.HandleApiCommand);
                _apiService.RouteRequest($"component/{component.Id}/status", component.HandleApiRequest);
                component.StateChanged += (s, e) => _apiService.NotifyStateChanged(component);
            }

            foreach (var automation in _automationService.GetAutomations())
            {
                new SettingsContainerApiDispatcher(automation.Settings, $"automation/{automation.Id}", _apiService).ExposeToApi();
            }
        }

        private void HandleApiGetStatus(IApiContext apiContext)
        {
            var result = new JsonObject();
            result.SetNamedString("type", "HA4IoT.Status");
            result.SetNamedNumber("version", 1D);

            var services = new JsonObject();
            foreach (var service in _controller.ServiceLocator.GetServices())
            {
                services.SetNamedObject(service.InterfaceType.Name, service.ServiceInstance.ExportStatusToJsonObject());
            }

            result.SetNamedValue("services", services);

            var components = new JsonObject();
            foreach (var component in _componentService.GetComponents())
            {
                components.SetNamedValue(component.Id.Value, component.ExportStatusToJsonObject());
            }

            result.SetNamedValue("components", components);

            var automations = new JsonObject();
            foreach (var automation in _automationService.GetAutomations())
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
            foreach (var area in _areaService.GetAreas())
            {
                areas.SetNamedValue(area.Id.Value, ExportAreaConfigurationToJsonValue(area));
            }

            configuration.SetNamedValue("areas", areas);
            configuration.SetNamedValue("controller", _controller.Settings.Export());

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
