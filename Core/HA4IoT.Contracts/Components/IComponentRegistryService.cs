using System.Collections.Generic;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Components
{
    public interface IComponentRegistryService : IService
    {
        void RegisterComponent(IComponent component);

        IComponent GetComponent(string id);
        
        TComponent GetComponent<TComponent>(string id) where TComponent : IComponent;

        IList<IComponent> GetComponents();

        IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent;
        
        bool ContainsComponent(string id);
    }
}
