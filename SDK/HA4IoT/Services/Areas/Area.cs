using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Services.Automations;
using HA4IoT.Services.Components;

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
        }

        public AreaId Id { get; }

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

        public IList<IAutomation> GetAutomations()
        {
            return _automations.GetAll();
        }
    }
}