using System;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Components
{
    public static class ComponentExtensions
    {
        public static bool SupportsState(this IComponent component, GenericComponentState componentState)
        {
            if (componentState == null) throw new ArgumentNullException(nameof(componentState));
            if (component == null) throw new ArgumentNullException(nameof(component));

            return component.GetSupportedStates().Any(s => s.Equals(componentState));
        }

        public static bool TryTurnOn(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            try
            {
                component.InvokeCommand(new TurnOnCommand());
            }
            catch (CommandNotSupportedException)
            {
                return false;
            }
            
            return true;
        }

        public static bool TryTurnOff(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            try
            {
                component.InvokeCommand(new TurnOffCommand());
            }
            catch (CommandNotSupportedException)
            {
                return false;
            }

            return true;
        }

        public static bool TryMoveUp(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TrySetState(component, RollerShutterStateId.MovingUp);
        }

        public static bool TryMoveDown(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return TrySetState(component, RollerShutterStateId.MovingDown);
        }

        public static bool TrySetState(this IComponent component, GenericComponentState state)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            var actuator = component as IActuator;
            if (actuator == null)
            {
                return false;
            }

            actuator.ChangeState(state);
            return true;
        }
    }
}
