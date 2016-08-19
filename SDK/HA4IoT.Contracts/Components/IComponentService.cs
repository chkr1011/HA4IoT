using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Components
{
    public interface IComponentService : IService
    {
        void AddComponent(IComponent component);

        TComponent GetComponent<TComponent>(ComponentId id) where TComponent : IComponent;

        TComponent GetComponent<TComponent>() where TComponent : IComponent;

        IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent;

        IList<IComponent> GetComponents();

        bool ContainsComponent(ComponentId componentId);
    }
}
