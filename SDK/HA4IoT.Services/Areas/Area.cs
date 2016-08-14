using System;
using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Components;

namespace HA4IoT.Services.Areas
{
    public class Area : IArea
    {
        private readonly ComponentCollection _components = new ComponentCollection();
        private readonly AutomationCollection _automations = new AutomationCollection();

        private readonly IComponentService _componentService;

        public Area(AreaId id, IComponentService componentService)
        {
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));

            _componentService = componentService;

            Id = id;
            
            Settings = new SettingsContainer(StoragePath.WithFilename("Areas", id.Value, "Settings.json"));
            GeneralSettingsWrapper = new AreaSettingsWrapper(Settings);
        }

        public AreaId Id { get; }

        public ISettingsContainer Settings { get; }

        public IAreaSettingsWrapper GeneralSettingsWrapper { get; }

        public IController AreaService { get; }

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
            AreaService.AddAutomation(automation);
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
    }
}