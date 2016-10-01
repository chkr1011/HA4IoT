using System;
using System.Linq;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Components
{
    public static class ComponentExtensions
    {
       public static bool SupportsState(this IComponent component, ComponentState componentState)
        {
            if (componentState == null) throw new ArgumentNullException(nameof(componentState));
            if (component == null) throw new ArgumentNullException(nameof(component));

            return component.GetSupportedStates().Any(s => s.Equals(componentState));
        }
    }
}
