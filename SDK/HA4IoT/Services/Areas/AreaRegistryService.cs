using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Services.Areas
{
    [ApiServiceClass(typeof(IAreaRegistryService))]
    public class AreaRegistryService : ServiceBase, IAreaRegistryService
    {
        private readonly Dictionary<string, IArea> _areas = new Dictionary<string, IArea>();

        private readonly IComponentRegistryService _componentService;
        private readonly IAutomationRegistryService _automationService;
        private readonly ISettingsService _settingsService;

        public AreaRegistryService(
            IComponentRegistryService componentService,
            IAutomationRegistryService automationService,
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService,
            IApiDispatcherService apiService,
            ISettingsService settingsService)
        {
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (automationService == null) throw new ArgumentNullException(nameof(automationService));
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _componentService = componentService;
            _automationService = automationService;
            _settingsService = settingsService;

            systemEventsService.StartupCompleted += (s, e) =>
            {
                systemInformationService.Set("Areas/Count", _areas.Count);
            };

            apiService.ConfigurationRequested += HandleApiConfigurationRequest;
        }

        public IArea RegisterArea(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var area = new Area(id, _componentService, _automationService, _settingsService);
            _areas.Add(area.Id, area);

            return area;
        }

        public IArea GetArea(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return _areas[id];
        }

        public IList<IArea> GetAreas()
        {
            return _areas.Values.ToList();
        }

        private void HandleApiConfigurationRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            var areas = new JObject();
            foreach (var area in _areas.Values)
            {
                areas[area.Id] = ExportAreaConfiguration(area);
            }

            e.Context.Result["Areas"] = areas;
        }

        private JObject ExportAreaConfiguration(IArea area)
        {
            var components = new JObject();
            foreach (var component in area.GetComponents())
            {
                var componentConfiguration = new JObject
                {
                    ["Type"] = component.GetType().Name,
                    ["Settings"] = _settingsService.GetRawComponentSettings(component.Id),
                    ["Features"] = JToken.FromObject(component.GetFeatures().Serialize())
                };

                var supportedStates = component.GetSupportedStates();
                if (supportedStates != null)
                {
                    var supportedStatesJson = new JArray();
                    foreach (var supportedState in supportedStates)
                    {
                        supportedStatesJson.Add(supportedState.JToken);
                    }

                    componentConfiguration["SupportedStates"] = supportedStatesJson;
                }
                
                components[component.Id] = componentConfiguration;
            }
            
            var automations = new JObject();
            foreach (var automation in area.GetAutomations())
            {
                var automationSettings = new JObject
                {
                    ["Type"] = automation.GetType().Name,
                    ["Settings"] = _settingsService.GetRawAutomationSettings(automation.Id)
                };

                automations[automation.Id] = automationSettings;
            }
            
            var configuration = new JObject
            {
                ["Settings"] = _settingsService.GetRawAreaSettings(area.Id),
                ["Components"] = components,
                ["Automations"] = automations
            };

            return configuration;
        }
    }
}
