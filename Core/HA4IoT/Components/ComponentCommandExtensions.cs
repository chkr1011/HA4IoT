using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Components
{
    public static class ComponentCommandExtensions
    {
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

        public static bool TryDecreaseLevel(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new DecreaseLevelCommand());
        }

        public static bool TrySetLevel(this IComponent component, int level)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new SetLevelCommand { Level = level });
        }

        public static bool TrySetState(this IComponent component, string state)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new SetStateCommand { Id = state });
        }

        public static bool TrySetNextState(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new SetNextStateCommand());
        }

        public static bool TrySetColor(this IComponent component, double hue, double saturation, double value)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return component.TryInvokeCommand(new SetColorCommand { Hue = hue, Saturation = saturation, Value = value });
        }

        public static bool TryInvokeCommand(this IComponent component, ICommand command)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (command == null) throw new ArgumentNullException(nameof(command));

            try
            {
                component.ExecuteCommand(command);
            }
            catch (CommandNotSupportedException exception)
            {
                Log.Default.Warning(exception, $"Error while invoking command for component '{component.Id}'. " + exception.Message);
                return false;
            }

            return true;
        }
    }
}
