using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Components
{
    public interface IComponentRegistryService : IService
    {
        void AddComponent(IComponent component);

        IComponent GetComponent(string id);

        TComponent GetComponent<TComponent>(string id) where TComponent : IComponent;

        TComponent GetComponent<TComponent>() where TComponent : IComponent;

        IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent;

        IList<IComponent> GetComponents();

        bool ContainsComponent(string id);
    }
}
