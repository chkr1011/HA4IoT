using System.Collections.Generic;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Areas
{
    public interface IArea
    {
        AreaId Id { get; }

        void AddComponent(IComponent component);

        bool ContainsComponent(ComponentId componentId);

        TComponent GetComponent<TComponent>(ComponentId componentId) where TComponent : IComponent;

        TComponent GetComponent<TComponent>() where TComponent : IComponent;

        IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent;

        IList<IComponent> GetComponents();

        void AddAutomation(IAutomation automation);

        IList<IAutomation> GetAutomations();
    }
}
