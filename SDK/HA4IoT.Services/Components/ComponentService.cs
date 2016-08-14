using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services.Components
{
    public class ComponentService : ServiceBase, IComponentService, IStartupCompletedNotification
    {
        private readonly ISystemInformationService _systemInformationService;
        private readonly ComponentCollection _components = new ComponentCollection();

        public ComponentService(ISystemInformationService systemInformationService)
        {
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            _systemInformationService = systemInformationService;
        }

        public void AddComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _components.AddUnique(component.Id, component);
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
            if (id == null) throw new ArgumentNullException(nameof(id));

            return _components.Get<TComponent>(id);
        }

        public void OnStartupCompleted()
        {
            _systemInformationService.Set("Components/Count", _components.GetAll().Count);
        }
    }
}
