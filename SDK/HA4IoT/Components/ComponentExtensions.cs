using System;
using System.Linq;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;

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
            return component.TryInvokeCommand(new TurnOnCommand());
        }

        public static bool TryTurnOff(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new TurnOffCommand());
        }

        public static bool TryTogglePowerState(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new TogglePowerStateCommand());
        }

        public static bool TryReset(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new ResetCommand());
        }
        
        public static bool TryMoveUp(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new MoveUpCommand());
        }

        public static bool TryMoveDown(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new MoveDownCommand());
        }

        public static bool TryIncreaseLevel(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new IncreaseLevelCommand());
        }

        public static bool TryInvokeCommand(this IComponent component, ICommand command)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (command == null) throw new ArgumentNullException(nameof(command));

            try
            {
                component.InvokeCommand(command);
            }
            catch (CommandNotSupportedException exception)
            {
                Log.Warning(exception, $"Error while invoking command for component '{component.Id}'. " + exception.Message);
                return false;
            }

            return true;
        }
    }
}
