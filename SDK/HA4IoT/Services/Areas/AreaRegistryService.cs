using System;
using System.Collections.Generic;
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
        private readonly AreaCollection _areas = new AreaCollection();

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
                systemInformationService.Set("Areas/Count", _areas.GetAll().Count);
            };

            apiService.ConfigurationRequested += HandleApiConfigurationRequest;
        }

        public IArea RegisterArea(AreaId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var area = new Area(id, _componentService, _automationService, _settingsService);
            _areas.AddUnique(area.Id, area);

            return area;
        }

        public IArea GetArea(AreaId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return _areas.Get(id);
        }

        public IList<IArea> GetAreas()
        {
            return _areas.GetAll();
        }

        private void HandleApiConfigurationRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            var areas = new JObject();
            foreach (var area in _areas.GetAll())
            {
                areas[area.Id.Value] = ExportAreaConfiguration(area);
            }

            e.Context.Result["Areas"] = areas;
        }

        private JObject ExportAreaConfiguration(IArea area)
        {
            var components = new JObject();
            foreach (var component in area.GetComponents())
            {
                var componentConfiguration = component.ExportConfiguration();
                componentConfiguration["Settings"] = _settingsService.GetRawSettings(component.Id);
                
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
                
                components[component.Id.Value] = componentConfiguration;
            }
            
            var automations = new JObject();
            foreach (var automation in area.GetAutomations())
            {
                var automationSettings = new JObject
                {
                    ["Type"] = automation.GetType().Name,
                    ["Settings"] = _settingsService.GetRawSettings(automation.Id)
                };

                automations[automation.Id.Value] = automationSettings;
            }
            
            var configuration = new JObject
            {
                ["Settings"] = _settingsService.GetRawSettings(area.Id),
                ["Components"] = components,
                ["Automations"] = automations
            };

            return configuration;
        }
    }
}
