using System.Collections.Generic;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Core
{
    public interface IComponentController
    {
        void AddComponent(IComponent component);

        TComponent GetComponent<TComponent>(ComponentId id) where TComponent : IComponent;

        TComponent GetComponent<TComponent>() where TComponent : IComponent;

        IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent;

        IList<IComponent> GetComponents();
    }
}
