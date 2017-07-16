using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Settings;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Areas
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
            ISystemInformationService systemInformationService,
            IApiDispatcherService apiService,
            ISettingsService settingsService,
            IScriptingService scriptingService)
        {
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));

            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));
            _automationService = automationService ?? throw new ArgumentNullException(nameof(automationService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            
            apiService.ConfigurationRequested += HandleApiConfigurationRequest;

            systemInformationService.Set("Areas/Count", () => _areas.Count);

            scriptingService.RegisterScriptProxy(s => new AreaRegistryScriptProxy(this));
        }
        
        public IArea RegisterArea(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var area = new Area(id, _componentService, _automationService, _settingsService);
            lock (_areas)
            {
                _areas.Add(area.Id, area);
            }
            
            return area;
        }

        public IArea GetArea(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            lock (_areas)
            {
                return _areas[id];
            }
        }

        public IList<IArea> GetAreas()
        {
            lock (_areas)
            {
                return _areas.Values.ToList();
            }
        }

        private void HandleApiConfigurationRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            var areas = new JObject();
            foreach (var area in GetAreas())
            {
                areas[area.Id] = ExportAreaConfiguration(area);
            }

            e.ApiContext.Result["Areas"] = areas;
        }

        private JObject ExportAreaConfiguration(IArea area)
        {
            var components = new JObject();
            foreach (var component in area.GetComponents())
            {
                var componentConfiguration = new JObject
                {
                    ["Type"] = component.GetType().Name,
                    ["Settings"] = _settingsService.GetRawSettings(component),
                    ["Features"] = JObject.FromObject(component.GetFeatures().Serialize()),
                };

                components[component.Id] = componentConfiguration;
            }
            
            var automations = new JObject();
            foreach (var automation in area.GetAutomations())
            {
                var automationSettings = new JObject
                {
                    ["Type"] = automation.GetType().Name,
                    ["Settings"] = _settingsService.GetRawSettings(automation)
                };

                automations[automation.Id] = automationSettings;
            }
            
            var configuration = new JObject
            {
                ["Settings"] = _settingsService.GetRawSettings(area),
                ["Components"] = components,
                ["Automations"] = automations
            };

            return configuration;
        }
    }
}
