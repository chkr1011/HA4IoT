using System.Collections.Generic;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Areas
{
    public interface IArea
    {
        string Id { get; }

        AreaSettings Settings { get; }

        void RegisterComponent(IComponent component);

        bool ContainsComponent(string id);

        IComponent GetComponent(string id);

        TComponent GetComponent<TComponent>(string id) where TComponent : IComponent;

        IList<IComponent> GetComponents();

        IList<TComponent> GetComponents<TComponent>() where TComponent : IComponent;
        
        void RegisterAutomation(IAutomation automation);

        IList<IAutomation> GetAutomations();
    }
}
