using System;
using System.Linq;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Components
{
    public static class ComponentExtensions
    {
        public static void Enable(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            component.GeneralSettingsWrapper.IsEnabled = true;
        }

        public static void Disable(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            component.GeneralSettingsWrapper.IsEnabled = false;
        }

        public static bool IsEnabled(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return component.GeneralSettingsWrapper.IsEnabled;
        }

        public static bool SupportsState(this IComponent component, IComponentState componentState)
        {
            if (componentState == null) throw new ArgumentNullException(nameof(componentState));
            if (component == null) throw new ArgumentNullException(nameof(component));

            return component.GetSupportedStates().Any(s => s.Equals(componentState));
        }
    }
}
