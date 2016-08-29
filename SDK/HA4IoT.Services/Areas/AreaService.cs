using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Networking.Json;

namespace HA4IoT.Services.Areas
{
    public class AreaService : ServiceBase, IAreaService
    {
        private readonly AreaCollection _areas = new AreaCollection();

        private readonly IComponentService _componentService;
        private readonly IAutomationService _automationService;
        private readonly IApiService _apiService;
        private readonly ISettingsService _settingsService;

        public AreaService(
            IComponentService componentService,
            IAutomationService automationService,
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService,
            IApiService apiService,
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
            _apiService = apiService;
            _settingsService = settingsService;

            systemEventsService.StartupCompleted += (s, e) =>
            {
                systemInformationService.Set("Areas/Count", _areas.GetAll().Count);
            };

            apiService.ConfigurationRequested += HandleApiConfigurationRequest;
        }

        public IArea CreateArea(AreaId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var area = new Area(id, _componentService, _automationService);
            AddArea(area);

            return area;
        }

        public void AddArea(IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            _areas.AddUnique(area.Id, area);
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
            var areas = new JsonObject();
            foreach (var area in _areas.GetAll())
            {
                areas.SetNamedValue(area.Id.Value, ExportAreaConfiguration(area));
            }

            e.Context.Response.SetNamedValue("Areas", areas);
        }

        private IJsonValue ExportAreaConfiguration(IArea area)
        {
            var configuration = new JsonObject();
            configuration.SetNamedValue("Settings", _settingsService.GetRawSettings(area.Id));
            
            var components = new JsonObject();
            foreach (var component in area.GetComponents())
            {
                var componentConfiguration = component.ExportConfiguration();
                //componentConfiguration.SetValue(ComponentConfigurationKey.Type, GetType().Name);
                componentConfiguration.SetValue(ComponentConfigurationKey.Settings, _settingsService.GetRawSettings(component.Id));
                
                var supportedStates = component.GetSupportedStates();
                if (supportedStates != null)
                {
                    var supportedStatesJson = new JsonArray();
                    foreach (var supportedState in supportedStates)
                    {
                        supportedStatesJson.Add(supportedState.ToJsonValue());
                    }

                    configuration.SetValue(ComponentConfigurationKey.SupportedStates, supportedStatesJson);
                }
                
                components.SetNamedValue(component.Id.Value, componentConfiguration);
            }

            configuration.SetNamedValue("Components", components);

            var automations = new JsonObject();
            foreach (var automation in area.GetAutomations())
            {
                var automationSettings = new JsonObject();
                automationSettings.SetValue("Settings", _settingsService.GetRawSettings(automation.Id));

                automations.SetValue(automation.Id.Value, automationSettings);
            }

            configuration.SetNamedValue("Automations", automations);

            return configuration;
        }
    }
}
