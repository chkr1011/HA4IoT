using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Components;
using HA4IoT.Settings;

namespace HA4IoT.Services.Areas
{
    public class Area : IArea
    {
        private readonly ComponentCollection _components = new ComponentCollection();
        private readonly AutomationCollection _automations = new AutomationCollection();

        private readonly IComponentService _componentService;
        private readonly IAutomationService _automationService;

        public Area(AreaId id, IComponentService componentService, IAutomationService automationService)
        {
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (automationService == null) throw new ArgumentNullException(nameof(automationService));

            _componentService = componentService;
            _automationService = automationService;

            Id = id;
            
            Settings = new SettingsContainer(StoragePath.WithFilename("Areas", id.Value, "Settings.json"));
            GeneralSettingsWrapper = new AreaSettingsWrapper(Settings);
        }

        public AreaId Id { get; }

        public ISettingsContainer Settings { get; }

        public IAreaSettingsWrapper GeneralSettingsWrapper { get; }
        
        public void AddComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _components.AddUnique(component.Id, component);
            _componentService.AddComponent(component);
        }

        public TComponent GetComponent<TComponent>() where TComponent : IComponent
        {
            return _components.Get<TComponent>();
        }

        public IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent
        {
            return _components.GetAll<TComponent>();
        }

        public IList<IComponent> GetComponents()
        {
            return _components.GetAll();
        }

        public bool ContainsComponent(ComponentId componentId)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            return _components.Contains(componentId);
        }

        public TComponent GetComponent<TComponent>(ComponentId id) where TComponent : IComponent
        {
            return _components.Get<TComponent>(id);
        }

        public void AddAutomation(IAutomation automation)
        {
            _automations.AddUnique(automation.Id, automation);
            _automationService.AddAutomation(automation);
        }

        public IList<TAutomation> GetAutomations<TAutomation>() where TAutomation : IAutomation
        {
            return _automations.GetAll<TAutomation>();
        }

        public TAutomation GetAutomation<TAutomation>(AutomationId id) where TAutomation : IAutomation
        {
            return _automations.Get<TAutomation>(id);
        }

        public IList<IAutomation> GetAutomations()
        {
            return _automations.GetAll();
        }

        public JsonObject ExportConfigurationToJsonObject()
        {
            return Settings.Export();
        }

        public JsonObject GetStatus()
        {
            return null;
        }

        public void HandleApiCall(IApiContext apiContext)
        {
        }
    }
}