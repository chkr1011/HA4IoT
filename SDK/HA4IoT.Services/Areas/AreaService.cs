using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Settings;

namespace HA4IoT.Services.Areas
{
    public class AreaService : ServiceBase, IAreaService
    {
        private readonly AreaCollection _areas = new AreaCollection();

        private readonly IComponentService _componentService;
        private readonly IAutomationService _automationService;
        private readonly IApiService _apiService;

        public AreaService(
            IComponentService componentService,
            IAutomationService automationService,
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService,
            IApiService apiService)
        {
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (automationService == null) throw new ArgumentNullException(nameof(automationService));
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            _componentService = componentService;
            _automationService = automationService;
            _apiService = apiService;

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

            new SettingsContainerApiDispatcher(area.Settings, $"area/{area.Id}", _apiService).ExposeToApi();
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
                areas.SetNamedValue(area.Id.Value, ExportAreaConfigurationToJsonValue(area));
            }

            e.Context.Response.SetNamedValue("areas", areas);
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
