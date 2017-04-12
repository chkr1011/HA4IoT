using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Exceptions;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Services.Areas
{
    public class Area : IArea
    {
        private readonly Dictionary<string, IComponent> _components = new Dictionary<string, IComponent>();
        private readonly Dictionary<string, IAutomation> _automations = new Dictionary<string, IAutomation>();

        private readonly IComponentRegistryService _componentService;
        private readonly IAutomationRegistryService _automationService;
        
        public Area(string id, IComponentRegistryService componentService, IAutomationRegistryService automationService, ISettingsService settingsService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));
            _automationService = automationService ?? throw new ArgumentNullException(nameof(automationService));

            Id = id;

            settingsService.CreateSettingsMonitor(this, s => Settings = s.NewSettings);
        }

        public string Id { get; }

        public AreaSettings Settings { get; private set; }

        public void RegisterComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            lock (_components)
            {
                _components.Add(component.Id, component);
            }

            _componentService.RegisterComponent(component);
        }

        public void RegisterAutomation(IAutomation automation)
        {
            lock (_automations)
            {
                _automations.Add(automation.Id, automation);
            }
            
            _automationService.AddAutomation(automation);
        }

        public IList<IComponent> GetComponents()
        {
            lock (_components)
            {
                return _components.Values.ToList();
            }
        }

        public IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent
        {
            lock (_components)
            {
                return _components.Values.OfType<TComponent>().ToList();
            }
        }
        
        public bool ContainsComponent(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            lock (_components)
            {
                return _components.ContainsKey(id);
            }
        }

        public IComponent GetComponent(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            lock (_components)
            {
                return _components[id];
            }
        }

        public TComponent GetComponent<TComponent>(string id) where TComponent : IComponent
        {
            if (!ContainsComponent(id)) throw new ComponentNotFoundException(id);

            lock (_components)
            {
                return (TComponent) _components[id];
            }
        }

        public IList<IAutomation> GetAutomations()
        {
            return _automations.Values.ToList();
        }
    }
}